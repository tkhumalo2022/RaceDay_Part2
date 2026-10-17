using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaCeDay.Data;
using RaCeDay.Models;

namespace RaCeDay.Tests;

public class PaymentWorkflowTests(IdentityFixture fixture) : IClassFixture<IdentityFixture>
{
    private async Task<HttpClient> Actor(string role)
    {
        var email = $"payments-{Guid.NewGuid():N}@example.invalid";
        using var scope = fixture.Factory.Services.CreateScope();
        var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = new ApplicationUser { UserName = email, Email = email, FirstName = "Test", LastName = role };
        Assert.True((await users.CreateAsync(user, "Local-Test-123!")).Succeeded);
        Assert.True((await users.AddToRoleAsync(user, role)).Succeeded);
        var client = fixture.Client();
        var token = await IdentityFixture.Token(client, "/Identity/Account/Login");
        var login = await client.PostAsync("/Identity/Account/Login",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["Input.Email"] = email, ["Input.Password"] = "Local-Test-123!",
                ["__RequestVerificationToken"] = token
            }));
        Assert.Equal(HttpStatusCode.Redirect, login.StatusCode);
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", await IdentityFixture.Token(client, "/Profile"));
        return client;
    }

    private static async Task<int> Created(HttpResponseMessage response)
    {
        var body = await response.Content.ReadAsStringAsync();
        Assert.True(response.StatusCode == HttpStatusCode.Created,
            $"Expected 201, got {(int)response.StatusCode}: {body}");
        return JsonDocument.Parse(body).RootElement.GetProperty("id").GetInt32();
    }

    private static async Task<(int EventId, int EntryId)> CreateEntry(HttpClient organiser, HttpClient participant)
    {
        var eventId = await Created(await organiser.PostAsJsonAsync("/api/events", new
        {
            eventName = "Test Payment Race", eventDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(25)),
            closingDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(10)),
            location = "Durban", eventType = "Run", status = "Open"
        }));
        var categoryId = await Created(await organiser.PostAsJsonAsync($"/api/events/{eventId}/categories",
            new { categoryName = "10 km", distanceKm = 10m, entryFee = 80m, maxParticipants = 5 }));
        var entryId = await Created(await participant.PostAsJsonAsync($"/api/events/{eventId}/enrolments", new { categoryId }));
        return (eventId, entryId);
    }

    [Fact]
    public async Task PendingAttemptsRespectOwnershipAmountsReferencesAndNeverClaimPaid()
    {
        using var organiser = await Actor("Organiser");
        using var otherOrganiser = await Actor("Organiser");
        using var participant = await Actor("Participant");
        using var otherParticipant = await Actor("Participant");
        var (_, entryId) = await CreateEntry(organiser, participant);
        var url = $"/api/enrolments/{entryId}/payments";
        var reference = "DEMO-" + Guid.NewGuid().ToString("N");

        Assert.Equal(HttpStatusCode.Forbidden, (await otherParticipant.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherOrganiser.GetAsync(url)).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await otherParticipant.PostAsJsonAsync(url,
            new { amount = 80m, paymentMethod = "EFT" })).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await organiser.PostAsJsonAsync(url,
            new { amount = 80m, paymentMethod = "EFT" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await participant.PostAsJsonAsync(url,
            new { amount = 79m, paymentMethod = "EFT" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await participant.PostAsJsonAsync(url,
            new { amount = 80m, paymentMethod = "Cryptocurrency" })).StatusCode);

        var response = await participant.PostAsJsonAsync(url,
            new { amount = 80m, paymentMethod = "EFT", transactionReference = reference, status = "Paid" });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var receipt = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Pending", receipt.GetProperty("status").GetString());
        Assert.Equal(80m, receipt.GetProperty("amount").GetDecimal());

        Assert.Equal(HttpStatusCode.Conflict, (await participant.PostAsJsonAsync(url,
            new { amount = 80m, paymentMethod = "EFT", transactionReference = reference })).StatusCode);
        Assert.Equal(HttpStatusCode.Created, (await participant.PostAsJsonAsync(url,
            new { amount = 80m, paymentMethod = "Cash" })).StatusCode);
        var participantAttempts = await participant.GetFromJsonAsync<JsonElement>(url);
        Assert.Equal(2, participantAttempts.GetArrayLength());
        Assert.All(participantAttempts.EnumerateArray(), p =>
            Assert.Equal("Pending", p.GetProperty("status").GetString()));
        var organiserAttempts = await organiser.GetFromJsonAsync<JsonElement>(url);
        Assert.Equal(2, organiserAttempts.GetArrayLength());

        Assert.Equal(HttpStatusCode.OK, (await participant.GetAsync($"/Payments/Index?enrolmentId={entryId}")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await participant.GetAsync($"/Payments/Create?enrolmentId={entryId}")).StatusCode);
        participant.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        Assert.Equal(HttpStatusCode.BadRequest, (await participant.PostAsJsonAsync(url,
            new { amount = 80m, paymentMethod = "EFT" })).StatusCode);
    }

    [Fact]
    public async Task ASettledPaymentPreventsNewAttemptsAndDatabasePreventsDoublePaid()
    {
        using var organiser = await Actor("Organiser");
        using var participant = await Actor("Participant");
        var (_, entryId) = await CreateEntry(organiser, participant);
        using (var scope = fixture.Factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Payments.Add(new PaymentAttempt
            {
                EnrolmentId = entryId, Amount = 80m, PaymentMethod = "EFT",
                Status = "Paid", TransactionReference = "TEST-SETTLED-" + Guid.NewGuid().ToString("N")
            });
            await db.SaveChangesAsync();
            db.Payments.Add(new PaymentAttempt
            {
                EnrolmentId = entryId, Amount = 80m, PaymentMethod = "Cash",
                Status = "Paid", TransactionReference = "TEST-SECOND-" + Guid.NewGuid().ToString("N")
            });
            await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
        }
        Assert.Equal(HttpStatusCode.Conflict, (await participant.PostAsJsonAsync(
            $"/api/enrolments/{entryId}/payments", new { amount = 80m, paymentMethod = "EFT" })).StatusCode);
    }
}
