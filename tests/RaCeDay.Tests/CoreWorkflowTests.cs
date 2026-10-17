using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaCeDay.Models;

namespace RaCeDay.Tests;

public class CoreWorkflowTests(IdentityFixture fixture) : IClassFixture<IdentityFixture>
{
    private async Task<HttpClient> Actor(string role)
    {
        var email = $"core-{Guid.NewGuid():N}@example.invalid";
        using var scope = fixture.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = email, Email = email, FirstName = "Core", LastName = role };
        Assert.True((await users.CreateAsync(user, "Local-Test-123!")).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        var client = fixture.Client();
        var token = await IdentityFixture.Token(client, "/Identity/Account/Login");
        var login = await client.PostAsync("/Identity/Account/Login", new FormUrlEncodedContent(new Dictionary<string,string>
        { ["Input.Email"] = email, ["Input.Password"] = "Local-Test-123!", ["__RequestVerificationToken"] = token }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", await IdentityFixture.Token(client, "/Profile"));
        return client;
    }
    private static object Event(string status = "Open") => new
    {
        eventName = "Durban Test Race", description = "Integration fixture", location = "Durban",
        eventType = "Run", eventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(20)),
        closingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)), status
    };
    private static object Category(string name = "5 km", int capacity = 2) => new
        { categoryName = name, distanceKm = 5m, entryFee = 50m, maxParticipants = capacity };
    private static async Task<int> Created(HttpResponseMessage response)
    {
        var text = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created, $"Expected 201, got {response.StatusCode}: {text}");
        return JsonDocument.Parse(text).RootElement.GetProperty("id").GetInt32();
    }
    private static async Task<int> NewEvent(HttpClient client) => await Created(await client.PostAsJsonAsync("/api/events", Event()));
    private static async Task<int> NewCategory(HttpClient client, int eventId, string name = "5 km", int capacity = 2) =>
        await Created(await client.PostAsJsonAsync($"/api/events/{eventId}/categories", Category(name, capacity)));

    [Fact]
    public async Task CompleteWorkflowPreservesOwnershipAndParticipantPrivacy()
    {
        using var owner = await Actor("Organiser");
        using var stranger = await Actor("Organiser");
        using var participant = await Actor("Participant");
        using var other = await Actor("Participant");
        var eventId = await NewEvent(owner);
        var categoryId = await NewCategory(owner, eventId);
        Assert.Equal(HttpStatusCode.OK, (await participant.GetAsync($"/api/events/{eventId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.PutAsJsonAsync($"/api/events/{eventId}", Event())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.DeleteAsync($"/api/events/{eventId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.PostAsJsonAsync($"/api/events/{eventId}/categories", Category())).StatusCode);
        var entry = await Created(await participant.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId }));
        var secondCategory = await NewCategory(owner, eventId, "10 km");
        await Created(await participant.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId = secondCategory }));
        Assert.Equal(HttpStatusCode.Conflict, (await participant.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.GetAsync($"/api/events/{eventId}/enrolments")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await owner.DeleteAsync($"/api/events/{eventId}")).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await owner.DeleteAsync($"/api/categories/{categoryId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.PutAsJsonAsync($"/api/events/{eventId}", Event("Closed"))).StatusCode);
        var result = new { startTime = "08:00:00", finishTime = "08:30:00", overallPosition = 1, resultStatus = "Finished" };
        Assert.Equal(HttpStatusCode.Forbidden, (await stranger.PostAsJsonAsync($"/api/enrolments/{entry}/results", result)).StatusCode);
        await Created(await owner.PostAsJsonAsync($"/api/enrolments/{entry}/results", result));
        Assert.Equal(HttpStatusCode.Conflict, (await owner.PostAsJsonAsync($"/api/enrolments/{entry}/results", result)).StatusCode);
        var mine = await participant.GetFromJsonAsync<JsonElement>("/api/results/me");
        Assert.Single(mine.EnumerateArray());
        Assert.Equal("Finished", mine[0].GetProperty("resultStatus").GetString());
        Assert.Empty((await other.GetFromJsonAsync<JsonElement>("/api/results/me")).EnumerateArray());
        Assert.Empty((await other.GetFromJsonAsync<JsonElement>("/api/enrolments/me")).EnumerateArray());
        Assert.Equal(HttpStatusCode.OK, (await participant.GetAsync("/Entries")).StatusCode);
        Assert.Contains("08:30", await participant.GetStringAsync("/Entries/Results"));
    }

    [Fact]
    public async Task CategoryCrudAndEventDeletionRespectOwnerAndValidation()
    {
        using var owner = await Actor("Organiser"); using var other = await Actor("Organiser");
        var id = await NewEvent(owner); var category = await NewCategory(owner, id);
        Assert.Equal(HttpStatusCode.Conflict, (await owner.PostAsJsonAsync($"/api/events/{id}/categories", Category())).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.PutAsJsonAsync($"/api/categories/{category}", Category("Changed"))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await other.DeleteAsync($"/api/categories/{category}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await owner.PutAsJsonAsync($"/api/categories/{category}", Category("Changed"))).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await owner.DeleteAsync($"/api/categories/{category}")).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await owner.DeleteAsync($"/api/events/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await owner.GetAsync($"/api/events/{id}")).StatusCode);
    }

    [Fact]
    public async Task EnrolmentRejectsWrongEventClosedEventAndFullCategory()
    {
        using var owner = await Actor("Organiser"); using var p1 = await Actor("Participant"); using var p2 = await Actor("Participant");
        var id = await NewEvent(owner); var otherId = await NewEvent(owner);
        var categoryId = await NewCategory(owner, id, capacity: 1);
        Assert.Equal(HttpStatusCode.BadRequest, (await p1.PostAsJsonAsync($"/api/events/{otherId}/enrolments", new { categoryId })).StatusCode);
        await Created(await p1.PostAsJsonAsync($"/api/events/{id}/enrolments", new { categoryId }));
        Assert.Equal(HttpStatusCode.Conflict, (await p2.PostAsJsonAsync($"/api/events/{id}/enrolments", new { categoryId })).StatusCode);
        await owner.PutAsJsonAsync($"/api/events/{id}", Event("Closed"));
        Assert.Equal(HttpStatusCode.Conflict, (await p2.PostAsJsonAsync($"/api/events/{id}/enrolments", new { categoryId })).StatusCode);
    }

    [Fact]
    public async Task Part1CategoryCountsAreStoredAndUpdatedBySqlServerTrigger()
    {
        using var owner = await Actor("Organiser");
        using var participant = await Actor("Participant");
        var raceId = await NewEvent(owner);
        var categoryId = await NewCategory(owner, raceId, capacity: 1);
        var before = await owner.GetFromJsonAsync<JsonElement>($"/api/events/{raceId}/categories");
        Assert.Equal(0, before[0].GetProperty("currentParticipants").GetInt32());

        var entryId = await Created(await participant.PostAsJsonAsync(
            $"/api/events/{raceId}/enrolments", new { categoryId }));
        var after = await owner.GetFromJsonAsync<JsonElement>($"/api/events/{raceId}/categories");
        Assert.Equal(1, after[0].GetProperty("currentParticipants").GetInt32());

        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaCeDay.Data.ApplicationDbContext>();
            Assert.Equal(1, (await db.Categories.FindAsync(categoryId))!.CurrentParticipants);
        }

        Assert.Equal(HttpStatusCode.OK,
            (await owner.PutAsJsonAsync($"/api/events/{raceId}", Event("Closed"))).StatusCode);
        await Created(await owner.PostAsJsonAsync($"/api/enrolments/{entryId}/results", new
        {
            startTime = "08:00:00", finishTime = "08:25:00",
            overallPosition = 1, resultStatus = "Finished"
        }));
        var finished = await owner.GetFromJsonAsync<JsonElement>($"/api/events/{raceId}/categories");
        // The Part 1 SQL trigger counts Registered, not completed, entries.
        Assert.Equal(0, finished[0].GetProperty("currentParticipants").GetInt32());
    }

    [Fact]
    public async Task Part1CategoryTriggerRecountsWhenAnEntryIsDeleted()
    {
        using var owner = await Actor("Organiser");
        using var participant = await Actor("Participant");
        var eventId = await NewEvent(owner);
        var categoryId = await NewCategory(owner, eventId);
        var entryId = await Created(await participant.PostAsJsonAsync(
            $"/api/events/{eventId}/enrolments", new { categoryId }));

        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaCeDay.Data.ApplicationDbContext>();
            var entry = await db.Enrolments.SingleAsync(e => e.Id == entryId);
            db.Enrolments.Remove(entry);
            await db.SaveChangesAsync();
        }

        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<RaCeDay.Data.ApplicationDbContext>();
            Assert.Equal(0, await db.Categories.AsNoTracking()
                .Where(c => c.Id == categoryId)
                .Select(c => c.CurrentParticipants).SingleAsync());
        }
    }

    [Fact]
    public async Task ConcurrentLastPlaceRequestsCannotOverbook()
    {
        using var owner = await Actor("Organiser"); using var p1 = await Actor("Participant"); using var p2 = await Actor("Participant");
        var id = await NewEvent(owner); var categoryId = await NewCategory(owner, id, capacity: 1);
        var responses = await Task.WhenAll(p1.PostAsJsonAsync($"/api/events/{id}/enrolments", new { categoryId }), p2.PostAsJsonAsync($"/api/events/{id}/enrolments", new { categoryId }));
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Created);
        Assert.Single(responses, r => r.StatusCode == HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task ApiRequiresAuthenticationRoleAndCsrf()
    {
        using var anonymous = fixture.Client();
        Assert.Equal(HttpStatusCode.Unauthorized, (await anonymous.GetAsync("/api/events")).StatusCode);
        using var p = await Actor("Participant");
        Assert.Equal(HttpStatusCode.Forbidden, (await p.PostAsJsonAsync("/api/events", Event())).StatusCode);
        using var owner = await Actor("Organiser"); owner.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.PostAsJsonAsync("/api/events", Event())).StatusCode);
    }

    [Fact]
    public async Task InvalidEventCategoryAndResultAreRejected()
    {
        using var owner = await Actor("Organiser"); using var p = await Actor("Participant");
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.PostAsJsonAsync("/api/events", new { eventName = "", eventType = "Boat" })).StatusCode);
        var id = await NewEvent(owner);
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.PostAsJsonAsync($"/api/events/{id}/categories", new { categoryName = "Bad", distanceKm = -1, entryFee = -1, maxParticipants = 0 })).StatusCode);
        var categoryId = await NewCategory(owner, id);
        var entry = await Created(await p.PostAsJsonAsync($"/api/events/{id}/enrolments", new { categoryId }));
        await owner.PutAsJsonAsync($"/api/events/{id}", Event("Closed"));
        Assert.Equal(HttpStatusCode.BadRequest, (await owner.PostAsJsonAsync($"/api/enrolments/{entry}/results", new { startTime = "09:00:00", finishTime = "08:00:00", overallPosition = 0, resultStatus = "Finished" })).StatusCode);
    }

    [Fact]
    public async Task MvcRacePagesRenderAndBlockOtherOrganisers()
    {
        using var organiser = await Actor("Organiser");
        using var stranger = await Actor("Organiser");
        using var participant = await Actor("Participant");
        var eventId = await NewEvent(organiser);
        var categoryId = await NewCategory(organiser, eventId);
        foreach (var path in new[] {
            "/Events", "/Events/Create", $"/Events/Edit/{eventId}",
            $"/Events/Details/{eventId}", $"/Categories/Create?eventId={eventId}",
            $"/Categories/Edit/{categoryId}", "/Home/Error" })
        {
            var response = await organiser.GetAsync(path);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        Assert.Equal(HttpStatusCode.OK, (await participant.GetAsync($"/Events/Details/{eventId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await participant.GetAsync("/Entries")).StatusCode);
        var denied = await stranger.GetAsync($"/Events/Edit/{eventId}");
        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Contains("AccessDenied", denied.Headers.Location?.OriginalString);
    }

    [Fact]
    public async Task SwaggerDescribesTheWorkflowEndpoints()
    {
        using var client = fixture.Client();
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/swagger/index.html")).StatusCode);
        var doc = await client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json");
        Assert.True(doc.GetProperty("paths").TryGetProperty("/api/events/{eventId}/enrolments", out _));
        Assert.True(doc.GetProperty("paths").TryGetProperty("/api/enrolments/{enrolmentId}/results", out _));
        var createEntry = doc.GetProperty("paths").GetProperty("/api/events/{eventId}/enrolments").GetProperty("post");
        Assert.Contains("Enrol", createEntry.GetProperty("summary").GetString());
        var login = doc.GetProperty("paths").GetProperty("/api/auth/login").GetProperty("post");
        Assert.Contains("cookie", login.GetProperty("summary").GetString(), StringComparison.OrdinalIgnoreCase);
    }
}
