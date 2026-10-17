using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
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
}
