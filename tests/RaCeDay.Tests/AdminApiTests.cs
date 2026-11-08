using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RaCeDay.Data;
using RaCeDay.Models;

namespace RaCeDay.Tests;

public class AdminApiTests(IdentityFixture fixture) : IClassFixture<IdentityFixture>
{
    private const string TestPassword = "Local-Test-123!";

    private async Task<ApplicationUser> AddUser(string role)
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser
        {
            FirstName = "Admin", LastName = role,
            UserName = $"{role}-{Guid.NewGuid():N}@example.invalid",
            EmailConfirmed = true,
            OrganiserName = role == "Organiser" ? "Test Race Club" : null
        };
        user.Email = user.UserName;
        Assert.True((await manager.CreateAsync(user, TestPassword)).Succeeded);
        Assert.True((await manager.AddToRoleAsync(user, role)).Succeeded);
        return user;
    }

    private static async Task SetToken(HttpClient client)
    {
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        var json = await client.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        Assert.True(client.DefaultRequestHeaders.TryAddWithoutValidation(
            "X-CSRF-TOKEN", json.GetProperty("requestToken").GetString()));
    }

    private static async Task SignIn(HttpClient client, ApplicationUser user)
    {
        await SetToken(client);
        var signedIn = await client.PostAsJsonAsync("/api/auth/login", new { email = user.Email, password = TestPassword });
        Assert.Equal(HttpStatusCode.OK, signedIn.StatusCode);
        await SetToken(client);
    }

    [Fact]
    public async Task AdminBoundariesAndAccessControlWorkWithRealSqlAccounts()
    {
        var admin = await AddUser("Admin");
        var participant = await AddUser("Participant");

        using var anonymous = fixture.Client();
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await anonymous.GetAsync("/api/admin/overview")).StatusCode);

        using var participantBrowser = fixture.Client();
        await SignIn(participantBrowser, participant);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await participantBrowser.GetAsync("/api/admin/overview")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden,
            (await participantBrowser.PostAsJsonAsync(
                $"/api/admin/users/{admin.Id}/access", new { disabled = true })).StatusCode);

        using var adminBrowser = fixture.Client();
        await SignIn(adminBrowser, admin);
        var summary = await adminBrowser.GetFromJsonAsync<JsonElement>("/api/admin/overview");
        Assert.True(summary.GetProperty("users").GetInt32() >= 2);
        Assert.True(summary.GetProperty("participants").GetInt32() >= 1);
        var users = await adminBrowser.GetFromJsonAsync<JsonElement>("/api/admin/users?role=Participant");
        Assert.True(users.GetProperty("total").GetInt32() >= 1);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await adminBrowser.GetAsync("/api/admin/users?role=SuperAdmin")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminBrowser.GetAsync("/api/admin/events")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await adminBrowser.GetAsync("/api/admin/enrolments")).StatusCode);

        Assert.Equal(HttpStatusCode.Conflict,
            (await adminBrowser.PostAsJsonAsync(
                $"/api/admin/users/{admin.Id}/access", new { disabled = true })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest,
            (await adminBrowser.PostAsJsonAsync(
                $"/api/admin/users/{participant.Id}/role", new { role = "Admin" })).StatusCode);

        // A logged-in user loses access immediately when an admin disables them.
        Assert.Equal(HttpStatusCode.OK,
            (await adminBrowser.PostAsJsonAsync(
                $"/api/admin/users/{participant.Id}/access", new { disabled = true })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            (await participantBrowser.GetAsync("/api/auth/session")).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await adminBrowser.PostAsJsonAsync(
                $"/api/admin/users/{participant.Id}/access", new { disabled = false })).StatusCode);

        // Role change is allowed only for accounts with no associated race data.
        Assert.Equal(HttpStatusCode.BadRequest,
            (await adminBrowser.PostAsJsonAsync(
                $"/api/admin/users/{participant.Id}/role", new { role = "Organiser" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await adminBrowser.PostAsJsonAsync(
                $"/api/admin/users/{participant.Id}/role",
                new { role = "Organiser", organiserName = "Test Sports Club" })).StatusCode);
        using var scope = fixture.Factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var changed = await manager.FindByIdAsync(participant.Id);
        Assert.NotNull(changed);
        Assert.True(await manager.IsInRoleAsync(changed, "Organiser"));
        Assert.False(await manager.IsInRoleAsync(changed, "Participant"));
        Assert.Equal("Test Sports Club", changed.OrganiserName);
    }

    [Fact]
    public async Task ExistingRaceDataBlocksRoleChangesAndReportsAreReadOnly()
    {
        var admin = await AddUser("Admin");
        var organiser = await AddUser("Organiser");
        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Events.Add(new RaceEvent
            {
                OrganiserId = organiser.Id, EventName = "QA Trail Run",
                EventDate = new DateOnly(2027, 2, 14), ClosingDate = new DateOnly(2027, 2, 12),
                EventType = "Run", Status = "Open", Location = "Durban"
            });
            await db.SaveChangesAsync();
        }
        using var client = fixture.Client();
        await SignIn(client, admin);
        var overview = await client.GetFromJsonAsync<JsonElement>("/api/admin/overview");
        Assert.True(overview.GetProperty("events").GetInt32() > 0);
        var eventPage = await client.GetFromJsonAsync<JsonElement>("/api/admin/events?query=Trail");
        Assert.True(eventPage.GetProperty("total").GetInt32() > 0);
        Assert.Equal(HttpStatusCode.Conflict,
            (await client.PostAsJsonAsync(
                $"/api/admin/users/{organiser.Id}/role",
                new { role = "Participant" })).StatusCode);
    }
}
