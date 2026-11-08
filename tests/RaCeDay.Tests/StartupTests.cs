using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RaCeDay.Controllers;

namespace RaCeDay.Tests;

public class StartupTests
{
    [Fact]
    public async Task StartingWebServerDoesNotConnectToOrMigrateDatabase()
    {
        await using var factory = new WebApplicationFactory<HomeController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:DefaultConnection",
                "Server=127.0.0.1,1;Database=MustNotBeTouched;User ID=unused;Password=unused;Connect Timeout=1;TrustServerCertificate=True");
        });
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        var response = await client.GetAsync("/");
        Assert.True(response.IsSuccessStatusCode);
    }

    [Theory]
    [InlineData("Development", false, false)]
    [InlineData("Staging", false, true)]
    [InlineData("Staging", true, false)]
    [InlineData("Production", true, true)]
    public async Task EmailConfirmationIsRelaxedOnlyForExplicitStagingReview(
        string environment, bool allowStagingTestAccounts, bool requiresConfirmation)
    {
        await using var factory = new WebApplicationFactory<HomeController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment(environment);
            builder.UseSetting("ConnectionStrings:DefaultConnection",
                "Server=127.0.0.1,1;Database=MustNotBeTouched;User ID=unused;Password=unused;Connect Timeout=1;TrustServerCertificate=True");
            builder.UseSetting("RaceDay:AllowUnconfirmedReviewAccounts", allowStagingTestAccounts.ToString());
        });
        using var scope = factory.Services.CreateScope();
        var identity = scope.ServiceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value;
        Assert.Equal(requiresConfirmation, identity.SignIn.RequireConfirmedAccount);

        var auth = scope.ServiceProvider
            .GetRequiredService<IOptionsMonitor<CookieAuthenticationOptions>>()
            .Get(IdentityConstants.ApplicationScheme);
        var session = scope.ServiceProvider.GetRequiredService<IOptions<SessionOptions>>().Value;
        var secure = environment != "Development"
            ? CookieSecurePolicy.Always
            : CookieSecurePolicy.SameAsRequest;
        Assert.Equal(secure, auth.Cookie.SecurePolicy);
        Assert.Equal(secure, session.Cookie.SecurePolicy);
    }

    [Fact]
    public async Task StagingHealthIsAvailableButUnconfiguredSqlIsNotReady()
    {
        await using var factory = new WebApplicationFactory<HomeController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Staging");
            builder.UseSetting("ConnectionStrings:DefaultConnection",
                "Server=127.0.0.1,1;Database=MustNotBeTouched;User ID=unused;Password=unused;Connect Timeout=1;TrustServerCertificate=True");
        });
        using var client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost") });
        Assert.Equal(System.Net.HttpStatusCode.OK, (await client.GetAsync("/healthz")).StatusCode);
        Assert.Equal(System.Net.HttpStatusCode.ServiceUnavailable,
            (await client.GetAsync("/readyz")).StatusCode);
    }
}
