using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using RaCeDay.Controllers;

namespace RaCeDay.Tests;

public class DemoPrefillTests(IdentityFixture fixture) : IClassFixture<IdentityFixture>
{
    [Fact]
    public async Task RegistrationHasDevelopmentOnlyManualPresetsAndNeverProvidesPasswords()
    {
        using var client = fixture.Client();
        var page = await client.GetAsync("/Identity/Account/Register");
        Assert.Equal(HttpStatusCode.OK, page.StatusCode);

        var html = await page.Content.ReadAsStringAsync();
        Assert.Contains("Fill Organiser demo details", html);
        Assert.Contains("Fill Participant demo details", html);
        Assert.Contains("data-demo-preset=\"organiser\"", html);
        Assert.Contains("data-demo-preset=\"participant\"", html);
        Assert.Contains("/js/demo-prefill.js", html);
        Assert.DoesNotContain("RaceDay@2026!", html);

        var file = await client.GetAsync("/js/demo-prefill.js");
        Assert.Equal(HttpStatusCode.OK, file.StatusCode);
        var js = await file.Content.ReadAsStringAsync();
        Assert.Contains("button.closest(\"form\")", js);
        Assert.Contains("field.dispatchEvent(new Event(\"change\"", js);
        Assert.DoesNotContain("form.submit(", js);
        Assert.DoesNotContain("requestSubmit(", js);
        Assert.DoesNotContain("RaceDay@2026!", js);
    }

    [Fact]
    public async Task LoggedInOrganiserGetsEventAndCategoryDemoPresets()
    {
        using var client = fixture.Client();
        var email = $"prefill-organiser-{Guid.NewGuid():N}@example.invalid";
        var form = new Dictionary<string, string>
        {
            ["Input.FirstName"] = "Test",
            ["Input.LastName"] = "Organiser",
            ["Input.Email"] = email,
            ["Input.Password"] = "Local-Test-123!",
            ["Input.ConfirmPassword"] = "Local-Test-123!",
            ["Input.Role"] = "Organiser",
            ["Input.OrganiserName"] = "Demo Group",
            ["__RequestVerificationToken"] = await IdentityFixture.Token(client, "/Identity/Account/Register")
        };
        var registration = await client.PostAsync("/Identity/Account/Register", new FormUrlEncodedContent(form));
        Assert.Equal(HttpStatusCode.Redirect, registration.StatusCode);

        var eventPage = await client.GetAsync("/Events/Create");
        Assert.Equal(HttpStatusCode.OK, eventPage.StatusCode);
        var eventHtml = await eventPage.Content.ReadAsStringAsync();
        Assert.Contains("Fill demo race details", eventHtml);
        Assert.Contains("data-demo-preset=\"event\"", eventHtml);
        Assert.Contains("/js/demo-prefill.js", eventHtml);
        Assert.Contains("type=\"button\" data-demo-preset", eventHtml);
        Assert.DoesNotContain("Set status to Closed (demo)", eventHtml);
    }
}
