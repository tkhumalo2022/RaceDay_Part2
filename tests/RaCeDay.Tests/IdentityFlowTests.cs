using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaCeDay.Controllers;
using RaCeDay.Data;
using RaCeDay.Models;

namespace RaCeDay.Tests;

// SQL Server integration tests use a NEW database each run and retain it for evidence.
public class IdentityFixture : IAsyncLifetime
{
    public string DatabaseName { get; } = "RaCeDayTests_" + Guid.NewGuid().ToString("N");
    public WebApplicationFactory<HomeController> Factory { get; private set; } = null!;
    public async Task InitializeAsync()
    {
        Factory = new WebApplicationFactory<HomeController>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("ConnectionStrings:DefaultConnection",
                $"Server=(localdb)\\MSSQLLocalDB;Database={DatabaseName};Trusted_Connection=True;TrustServerCertificate=True");
        });
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await db.Database.MigrateAsync();
        await IdentityRoleSeeder.SeedRolesAsync(scope.ServiceProvider);
        await IdentityRoleSeeder.SeedRolesAsync(scope.ServiceProvider);
        Console.WriteLine($"Retained integration database: {DatabaseName}");
    }
    public async Task DisposeAsync() => await Factory.DisposeAsync();
    public HttpClient Client() => Factory.CreateClient(new()
        { AllowAutoRedirect = false, BaseAddress = new Uri("https://localhost") });
    public static async Task<string> Token(HttpClient client, string path)
    {
        var html = await client.GetStringAsync(path);
        return WebUtility.HtmlDecode(Regex.Match(html,
            "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"").Groups[1].Value);
    }
}

public class IdentityFlowTests(IdentityFixture fixture) : IClassFixture<IdentityFixture>
{
    private static Dictionary<string, string> Registration(string email) => new()
    {
        ["Input.FirstName"] = "Test", ["Input.LastName"] = "Participant",
        ["Input.DateOfBirth"] = "2002-04-16",
        ["Input.Email"] = email, ["Input.Password"] = "Local-Test-123!",
        ["Input.ConfirmPassword"] = "Local-Test-123!", ["Input.Role"] = "Participant"
    };

    [Fact]
    public async Task RegistrationCannotGrantAdminAndLoginRejectsBadPassword()
    {
        using var client = fixture.Client();
        var email = $"participant-{Guid.NewGuid():N}@example.invalid";
        var form = Registration(email);
        form["__RequestVerificationToken"] = await IdentityFixture.Token(client, "/Identity/Account/Register");
        var registered = await client.PostAsync("/Identity/Account/Register?returnUrl=https://example.invalid", new FormUrlEncodedContent(form));
        Assert.Equal(HttpStatusCode.Redirect, registered.StatusCode);
        Assert.Equal("/", registered.Headers.Location?.OriginalString);
        using var scope = fixture.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal(new[] { "Participant" }, await users.GetRolesAsync(user));
        Assert.Equal(new DateOnly(2002, 4, 16), user.DateOfBirth);
        Assert.Null(user.OrganiserName);
        using var fresh = fixture.Client();
        var login = new Dictionary<string, string>
        {
            ["Input.Email"] = email, ["Input.Password"] = "Wrong-Password!",
            ["__RequestVerificationToken"] = await IdentityFixture.Token(fresh, "/Identity/Account/Login")
        };
        var rejected = await fresh.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(login));
        Assert.Equal(HttpStatusCode.OK, rejected.StatusCode);
        Assert.Contains("Invalid login attempt", await rejected.Content.ReadAsStringAsync());
        login["Input.Password"] = "Local-Test-123!";
        login["__RequestVerificationToken"] = await IdentityFixture.Token(fresh, "/Identity/Account/Login");
        Assert.Equal(HttpStatusCode.Redirect, (await fresh.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(login))).StatusCode);
    }

    [Fact]
    public async Task ApiAuthRegistersLogsInAndOutUsingCookieSessionWithCsrf()
    {
        using var client = fixture.Client();
        var email = $"api-organiser-{Guid.NewGuid():N}@example.invalid";
        var csrf = await client.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", csrf.GetProperty("requestToken").GetString());
        var input = new {
            firstName = "API", lastName = "Organiser", email, role = "Organiser", organiserName = "Durban Race Club",
            password = "Local-Test-123!", confirmPassword = "Local-Test-123!"
        };
        var registered = await client.PostAsJsonAsync("/api/auth/register", input);
        Assert.Equal(HttpStatusCode.Created, registered.StatusCode);
        var confirmation = await registered.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Organiser", confirmation.GetProperty("role").GetString());
        Assert.Equal("SignedIn", confirmation.GetProperty("status").GetString());
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/users/me")).StatusCode);

        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        var signedInToken = await client.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", signedInToken.GetProperty("requestToken").GetString());
        var loggedOut = await client.PostAsync("/api/auth/logout", null);
        Assert.Equal(HttpStatusCode.NoContent, loggedOut.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/users/me")).StatusCode);

        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        var nextToken = await client.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", nextToken.GetProperty("requestToken").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync(
            "/api/auth/login", new { email, password = "Wrong-123!" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync(
            "/api/auth/login", new { email, password = "Local-Test-123!" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/users/me")).StatusCode);

        using var hostile = fixture.Client();
        var hostileCsrf = await hostile.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        hostile.DefaultRequestHeaders.Add("X-CSRF-TOKEN", hostileCsrf.GetProperty("requestToken").GetString());
        var invalidEmail = $"api-admin-{Guid.NewGuid():N}@example.invalid";
        Assert.Equal(HttpStatusCode.BadRequest, (await hostile.PostAsJsonAsync("/api/auth/register",
            new { firstName = "No", lastName = "Admin", email = invalidEmail, role = "Admin",
                  password = "Local-Test-123!", confirmPassword = "Local-Test-123!" })).StatusCode);
        using var scope = fixture.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Null(await users.FindByEmailAsync(invalidEmail));

        using var anonymous = fixture.Client();
        Assert.Equal(HttpStatusCode.BadRequest, (await anonymous.PostAsJsonAsync("/api/auth/register", input)).StatusCode);
    }

    [Fact]
    public async Task RegistrationOffersOrganiserAndParticipantButRejectsAdmin()
    {
        using var organiserClient = fixture.Client();
        var email = $"organiser-{Guid.NewGuid():N}@example.invalid";
        var form = Registration(email);
        form["Input.Role"] = "Organiser";
        form["Input.OrganiserName"] = "Durban Training Races";
        form["__RequestVerificationToken"] = await IdentityFixture.Token(organiserClient, "/Identity/Account/Register");
        Assert.Equal(HttpStatusCode.Redirect, (await organiserClient.PostAsync(
            "/Identity/Account/Register", new FormUrlEncodedContent(form))).StatusCode);

        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            Assert.NotNull(user);
            Assert.Equal(new[] { "Organiser" }, await users.GetRolesAsync(user));
            Assert.Equal("Durban Training Races", user.OrganiserName);
            Assert.Null(user.DateOfBirth);
        }
        Assert.Equal(HttpStatusCode.OK, (await organiserClient.GetAsync("/Dashboard/Organiser")).StatusCode);
        var blockedDashboard = await organiserClient.GetAsync("/Dashboard/Admin");
        Assert.Equal(HttpStatusCode.Redirect, blockedDashboard.StatusCode);
        Assert.Contains("AccessDenied", blockedDashboard.Headers.Location?.OriginalString);

        using var hostileClient = fixture.Client();
        var invalidEmail = $"admin-attempt-{Guid.NewGuid():N}@example.invalid";
        var hostileForm = Registration(invalidEmail);
        hostileForm["Input.Role"] = "Admin";
        hostileForm["__RequestVerificationToken"] = await IdentityFixture.Token(hostileClient, "/Identity/Account/Register");
        Assert.Equal(HttpStatusCode.OK, (await hostileClient.PostAsync(
            "/Identity/Account/Register", new FormUrlEncodedContent(hostileForm))).StatusCode);
        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            Assert.Null(await users.FindByEmailAsync(invalidEmail));
        }
    }

    [Fact]
    public async Task RegistrationRequiresRoleSpecificDetails()
    {
        using var client = fixture.Client();
        var participantEmail = $"missing-dob-{Guid.NewGuid():N}@example.invalid";
        var form = Registration(participantEmail);
        form.Remove("Input.DateOfBirth");
        form["__RequestVerificationToken"] = await IdentityFixture.Token(client, "/Identity/Account/Register");
        var invalidParticipant = await client.PostAsync(
            "/Identity/Account/Register", new FormUrlEncodedContent(form));
        Assert.Equal(HttpStatusCode.OK, invalidParticipant.StatusCode);
        Assert.Contains("valid date of birth", await invalidParticipant.Content.ReadAsStringAsync());

        using var organiserClient = fixture.Client();
        var organiserEmail = $"missing-org-{Guid.NewGuid():N}@example.invalid";
        var organiserForm = Registration(organiserEmail);
        organiserForm["Input.Role"] = "Organiser";
        organiserForm["__RequestVerificationToken"] =
            await IdentityFixture.Token(organiserClient, "/Identity/Account/Register");
        var invalidOrganiser = await organiserClient.PostAsync(
            "/Identity/Account/Register", new FormUrlEncodedContent(organiserForm));
        Assert.Equal(HttpStatusCode.OK, invalidOrganiser.StatusCode);
        Assert.Contains("organisation name", await invalidOrganiser.Content.ReadAsStringAsync());

        using var scope = fixture.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        Assert.Null(await users.FindByEmailAsync(participantEmail));
        Assert.Null(await users.FindByEmailAsync(organiserEmail));
    }

    [Fact]
    public async Task OrganiserCanEditOwnOrganisationButCannotSetParticipantBirthDate()
    {
        using var client = fixture.Client();
        var email = $"organiser-profile-{Guid.NewGuid():N}@example.invalid";
        var form = Registration(email);
        form["Input.Role"] = "Organiser";
        form["Input.OrganiserName"] = "Original Club";
        form["__RequestVerificationToken"] =
            await IdentityFixture.Token(client, "/Identity/Account/Register");
        Assert.Equal(HttpStatusCode.Redirect,
            (await client.PostAsync("/Identity/Account/Register",
                new FormUrlEncodedContent(form))).StatusCode);

        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN",
            await IdentityFixture.Token(client, "/Profile"));
        var update = await client.PutAsJsonAsync("/api/users/me", new
        {
            firstName = "Test", lastName = "Organiser",
            organiserName = "Durban Road Runners", dateOfBirth = "2001-01-02",
            role = "Admin"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var profile = await update.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Durban Road Runners", profile.GetProperty("organiserName").GetString());
        Assert.Equal(JsonValueKind.Null, profile.GetProperty("dateOfBirth").ValueKind);

        using var scope = fixture.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal(new[] { "Organiser" }, await users.GetRolesAsync(user));
    }

    [Fact]
    public async Task ProfileApiOnlyUpdatesSignedInAllowlistedFieldsAndRequiresCsrf()
    {
        using var anonymous = fixture.Client();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/users/me")).StatusCode);
        using var client = fixture.Client();
        var email = $"profile-api-{Guid.NewGuid():N}@example.invalid";
        var form = Registration(email);
        form["__RequestVerificationToken"] = await IdentityFixture.Token(client, "/Identity/Account/Register");
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Identity/Account/Register",
            new FormUrlEncodedContent(form))).StatusCode);

        var before = await client.GetFromJsonAsync<JsonElement>("/api/users/me");
        Assert.Equal(email, before.GetProperty("email").GetString());
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", await IdentityFixture.Token(client, "/Profile"));
        var update = await client.PutAsJsonAsync("/api/users/me", new
        {
            firstName = "New", lastName = "Person", gender = "Other",
            phoneNumber = "0715551001", dateOfBirth = "2003-05-20", organiserName = "Fake Club",
            role = "Admin", email = "forged@example.invalid", id = "forged-id"
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        var after = await update.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("New", after.GetProperty("firstName").GetString());
        Assert.Equal("2003-05-20", after.GetProperty("dateOfBirth").GetString());
        Assert.Equal(JsonValueKind.Null, after.GetProperty("organiserName").ValueKind);
        Assert.Equal(email, after.GetProperty("email").GetString());
        Assert.Equal(before.GetProperty("id").GetString(), after.GetProperty("id").GetString());

        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await users.FindByEmailAsync(email);
            Assert.NotNull(user);
            Assert.Equal(new[] { "Participant" }, await users.GetRolesAsync(user));
        }
        var invalid = await client.PutAsJsonAsync("/api/users/me", new { firstName = "", lastName = "Valid" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PutAsJsonAsync("/api/users/me",
            new { firstName = "Other", lastName = "Valid" })).StatusCode);
    }

    [Fact]
    public async Task MissingPasswordConfirmationDoesNotCreateAccount()
    {
        using var client = fixture.Client();
        var email = $"invalid-{Guid.NewGuid():N}@example.invalid";
        var form = Registration(email);
        form.Remove("Input.ConfirmPassword");
        form["__RequestVerificationToken"] = await IdentityFixture.Token(client, "/Identity/Account/Register");
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(form))).StatusCode);
        using var scope = fixture.Factory.Services.CreateScope();
        Assert.Null(await scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>().FindByEmailAsync(email));
    }

    [Fact]
    public async Task RoleSeedingIsIdempotent()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        Assert.Equal(new[] { "Admin", "Organiser", "Participant" }, await db.Roles.OrderBy(r => r.Name).Select(r => r.Name).ToArrayAsync());
    }

    [Theory]
    [InlineData("Participant")]
    [InlineData("Organiser")]
    [InlineData("Admin")]
    public async Task EachRoleCanOpenOnlyItsDashboard(string role)
    {
        var email = $"role-{Guid.NewGuid():N}@example.invalid";
        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser { UserName = email, Email = email, FirstName = "Role", LastName = "Test" };
            Assert.True((await users.CreateAsync(user, "Local-Test-123!")).Succeeded);
            Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        }
        using var client = fixture.Client();
        await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string,string>
        {
            ["Input.Email"] = email, ["Input.Password"] = "Local-Test-123!",
            ["__RequestVerificationToken"] = await IdentityFixture.Token(client, "/Identity/Account/Login")
        }));
        foreach (var destination in new[] { "Participant", "Organiser", "Admin" })
        {
            var response = await client.GetAsync("/Dashboard/" + destination);
            if (destination == role) Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            else
            {
                Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
                Assert.Contains("AccessDenied", response.Headers.Location?.OriginalString);
            }
        }
    }

    [Fact]
    public async Task ProfileUpdatesCurrentUserWithoutAcceptingRoleOrEmailChanges()
    {
        using var client = fixture.Client();
        var email = $"profile-{Guid.NewGuid():N}@example.invalid";
        var form = Registration(email);
        form["__RequestVerificationToken"] = await IdentityFixture.Token(client, "/Identity/Account/Register");
        await client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(form));
        var update = new Dictionary<string,string>
        {
            ["FirstName"] = " Updated ", ["LastName"] = " Person ",
            ["Gender"] = "", ["PhoneNumber"] = "", ["Role"] = "Admin",
            ["Email"] = "other@example.invalid", ["Id"] = "someone-else",
            ["__RequestVerificationToken"] = await IdentityFixture.Token(client, "/Profile")
        };
        Assert.Equal(HttpStatusCode.Redirect, (await client.PostAsync("/Profile", new FormUrlEncodedContent(update))).StatusCode);
        using var scope = fixture.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await users.FindByEmailAsync(email);
        Assert.NotNull(user);
        Assert.Equal("Updated", user.FirstName);
        Assert.Equal("Person", user.LastName);
        Assert.Equal(new[] { "Participant" }, await users.GetRolesAsync(user));
        update["FirstName"] = " ";
        update["__RequestVerificationToken"] = await IdentityFixture.Token(client, "/Profile");
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsync("/Profile", new FormUrlEncodedContent(update))).StatusCode);
        update.Remove("__RequestVerificationToken");
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsync("/Profile", new FormUrlEncodedContent(update))).StatusCode);
    }

    [Theory]
    [InlineData("/Dashboard/Participant")]
    [InlineData("/Dashboard/Organiser")]
    [InlineData("/Dashboard/Admin")]
    [InlineData("/Profile")]
    public async Task AnonymousCannotAccessPrivatePages(string path)
    {
        using var client = fixture.Client();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Identity/Account/Login", response.Headers.Location?.OriginalString);
    }
}
