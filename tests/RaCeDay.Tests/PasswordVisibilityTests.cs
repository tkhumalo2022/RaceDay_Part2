using System.Net;
using RaCeDay.Controllers;
using Microsoft.AspNetCore.Mvc.Testing;

namespace RaCeDay.Tests;

public class PasswordVisibilityTests(IdentityFixture fixture) : IClassFixture<IdentityFixture>
{
    [Fact]
    public async Task RegistrationPageShowsToggleForBothPasswordFields()
    {
        using var client = fixture.Client();
        var response = await client.GetAsync("/Identity/Account/Register");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Show passwords", html);
        Assert.Contains("data-password-targets=\"Input_Password Input_ConfirmPassword\"", html);
        Assert.Contains("/js/password-visibility.js", html);
        Assert.Contains("type=\"password\"", html);
    }

    [Fact]
    public async Task LoginPageShowsPasswordToggleAndScriptIsServed()
    {
        using var client = fixture.Client();
        var response = await client.GetAsync("/Identity/Account/Login");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var html = await response.Content.ReadAsStringAsync();
        Assert.Contains("Show password", html);
        Assert.Contains("data-password-targets=\"Input_Password\"", html);
        Assert.Contains("/js/password-visibility.js", html);
        var script = await client.GetAsync("/js/password-visibility.js");
        Assert.Equal(HttpStatusCode.OK, script.StatusCode);
        var code = await script.Content.ReadAsStringAsync();
        Assert.Contains("toggle.addEventListener(\"change\"", code);
        Assert.Contains("toggle.checked ? \"text\" : \"password\"", code);
    }
}
