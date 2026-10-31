using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using RaCeDay.Portal.Models;

namespace RaCeDay.Portal.Services;

// The MVC frontend has no SQL connection, EF Core models or data-writing logic.
public sealed class RaceDayApiClient(HttpClient client)
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static void ForwardCookies(HttpRequest browser, HttpRequestMessage request)
    {
        if (browser.Headers.Cookie.Count > 0)
            request.Headers.TryAddWithoutValidation("Cookie", browser.Headers.Cookie.ToString());
    }
    private static void RelayCookies(HttpResponse browser, HttpResponseMessage api)
    {
        if (api.Headers.TryGetValues("Set-Cookie", out var cookies))
            foreach (var cookie in cookies) browser.Headers.Append("Set-Cookie", cookie);
    }
    public async Task<(T? Data, HttpStatusCode Status)> GetAsync<T>(
        string path, HttpRequest browser, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        ForwardCookies(browser, request);
        using var response = await client.SendAsync(request, ct);
        if (!response.IsSuccessStatusCode) return (default, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<T>(Json, ct), response.StatusCode);
    }
    public Task<(ApiSession? Data, HttpStatusCode Status)> SessionAsync(HttpRequest browser, CancellationToken ct)
        => GetAsync<ApiSession>("api/auth/session", browser, ct);

    // API-owned CSRF and Identity cookies. The portal relays them for same-host local development.
    public async Task<HttpStatusCode> AuthAsync<T>(
        string action, T input, HttpContext browser, CancellationToken ct)
    {
        if (action is not ("login" or "register" or "logout"))
            throw new ArgumentException("Invalid authentication operation.", nameof(action));
        using var csrfRequest = new HttpRequestMessage(HttpMethod.Get, "api/auth/antiforgery");
        ForwardCookies(browser.Request, csrfRequest);
        using var csrf = await client.SendAsync(csrfRequest, ct);
        if (!csrf.IsSuccessStatusCode) return csrf.StatusCode;
        using var json = await JsonDocument.ParseAsync(
            await csrf.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var token = json.RootElement.GetProperty("requestToken").GetString();
        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException("The backend did not return an antiforgery token.");
        using var post = new HttpRequestMessage(HttpMethod.Post, "api/auth/" + action);
        post.Content = JsonContent.Create(input, options: Json);
        post.Headers.TryAddWithoutValidation("X-CSRF-TOKEN", token);
        var jar = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var part in browser.Request.Headers.Cookie.ToString()
            .Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var pair = part.Trim();
            var index = pair.IndexOf('=');
            if (index > 0) jar[pair[..index]] = pair[(index + 1)..];
        }
        if (csrf.Headers.TryGetValues("Set-Cookie", out var newCookies))
            foreach (var value in newCookies)
            {
                var pair = value.Split(';', 2)[0].Trim();
                var index = pair.IndexOf('=');
                if (index > 0) jar[pair[..index]] = pair[(index + 1)..];
            }
        if (jar.Count > 0)
            post.Headers.TryAddWithoutValidation("Cookie",
                string.Join("; ", jar.Select(x => x.Key + "=" + x.Value)));
        RelayCookies(browser.Response, csrf);
        using var result = await client.SendAsync(post, ct);
        RelayCookies(browser.Response, result);
        return result.StatusCode;
    }
}
