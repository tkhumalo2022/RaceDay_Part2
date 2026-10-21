using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using RaCeDay.Models;

namespace RaCeDay.Tests;

public class ProfileAndAdminTests(IdentityFixture fixture) : IClassFixture<IdentityFixture>
{
    private static async Task RegisterParticipant(HttpClient client)
    {
        var form = new Dictionary<string, string>
        {
            ["Input.FirstName"] = "Photo",
            ["Input.LastName"] = "Tester",
            ["Input.DateOfBirth"] = "2002-02-02",
            ["Input.Email"] = $"photo-{Guid.NewGuid():N}@example.invalid",
            ["Input.Role"] = "Participant",
            ["Input.Password"] = "Local-Test-123!",
            ["Input.ConfirmPassword"] = "Local-Test-123!",
            ["__RequestVerificationToken"] =
                await IdentityFixture.Token(client, "/Identity/Account/Register")
        };
        var response = await client.PostAsync("/Identity/Account/Register",
            new FormUrlEncodedContent(form));
        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> Upload(HttpClient client, byte[] file,
        string filename, string mimeType)
    {
        var token = await IdentityFixture.Token(client, "/Profile");
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new StringContent(token), "__RequestVerificationToken");
        using var bytes = new ByteArrayContent(file);
        bytes.Headers.ContentType = MediaTypeHeaderValue.Parse(mimeType);
        multipart.Add(bytes, "image", filename);
        return await client.PostAsync("/Profile/UploadImage", multipart);
    }

    [Fact]
    public async Task ProfilePictureOnlyAcceptsImagesAndCanBeRemoved()
    {
        using var client = fixture.Client();
        await RegisterParticipant(client);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/Profile")).StatusCode);

        var fake = await Upload(client, "not really a png"u8.ToArray(), "portrait.png", "image/png");
        Assert.Equal(HttpStatusCode.OK, fake.StatusCode);
        Assert.Contains("not a valid JPG or PNG", await fake.Content.ReadAsStringAsync());

        var png = Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO2oaVMAAAAASUVORK5CYII=");
        var uploaded = await Upload(client, png, "my-image.png", "image/png");
        Assert.Equal(HttpStatusCode.Redirect, uploaded.StatusCode);
        var profile = await client.GetStringAsync("/Profile");
        var match = System.Text.RegularExpressions.Regex.Match(profile,
            @"src=""(/profile-images/[0-9a-f]{32}\.png)""");
        Assert.True(match.Success, "The profile should display the stored image.");
        var served = await client.GetAsync(match.Groups[1].Value);
        Assert.Equal(HttpStatusCode.OK, served.StatusCode);
        Assert.Equal("image/png", served.Content.Headers.ContentType?.MediaType);

        var removeToken = await IdentityFixture.Token(client, "/Profile");
        var removed = await client.PostAsync("/Profile/RemoveImage",
            new FormUrlEncodedContent(new Dictionary<string, string>
            { ["__RequestVerificationToken"] = removeToken }));
        Assert.Equal(HttpStatusCode.Redirect, removed.StatusCode);
        Assert.DoesNotContain(match.Groups[1].Value, await client.GetStringAsync("/Profile"));
        Assert.Equal(HttpStatusCode.NotFound,
            (await client.GetAsync(match.Groups[1].Value)).StatusCode);
    }

    [Fact]
    public async Task ProfileUploadNeedsAuthenticationAndAntiforgery()
    {
        using var anonymous = fixture.Client();
        using var multipart = new MultipartFormDataContent();
        multipart.Add(new ByteArrayContent("invalid"u8.ToArray()), "image", "test.png");
        var unauthorised = await anonymous.PostAsync("/Profile/UploadImage", multipart);
        Assert.Equal(HttpStatusCode.Redirect, unauthorised.StatusCode);

        using var signedIn = fixture.Client();
        await RegisterParticipant(signedIn);
        using var noToken = new MultipartFormDataContent();
        noToken.Add(new ByteArrayContent("invalid"u8.ToArray()), "image", "test.png");
        Assert.Equal(HttpStatusCode.BadRequest,
            (await signedIn.PostAsync("/Profile/UploadImage", noToken)).StatusCode);
    }

    [Fact]
    public async Task AdminDashboardShowsTotalsButRejectsParticipant()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var manager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var email = $"admin-{Guid.NewGuid():N}@example.invalid";
        var admin = new ApplicationUser
        { UserName = email, Email = email, FirstName = "Test", LastName = "Admin" };
        Assert.True((await manager.CreateAsync(admin, "Local-Test-123!")).Succeeded);
        Assert.True((await manager.AddToRoleAsync(admin, "Admin")).Succeeded);

        using var adminClient = fixture.Client();
        var antiforgery = await adminClient.GetFromJsonAsync<JsonElement>("/api/auth/antiforgery");
        adminClient.DefaultRequestHeaders.Add("X-CSRF-TOKEN",
            antiforgery.GetProperty("requestToken").GetString());
        Assert.Equal(HttpStatusCode.OK,
            (await adminClient.PostAsJsonAsync("/api/auth/login",
                new { email, password = "Local-Test-123!" })).StatusCode);
        var adminView = await adminClient.GetAsync("/Dashboard/Admin");
        Assert.Equal(HttpStatusCode.OK, adminView.StatusCode);
        var body = await adminView.Content.ReadAsStringAsync();
        Assert.Contains("Registered users", body);
        Assert.Contains("Payment attempts", body);

        using var participant = fixture.Client();
        await RegisterParticipant(participant);
        var denied = await participant.GetAsync("/Dashboard/Admin");
        Assert.Equal(HttpStatusCode.Redirect, denied.StatusCode);
        Assert.Contains("AccessDenied", denied.Headers.Location?.OriginalString);
    }
}
