using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using RaCeDay.Portal.Models;

namespace RaCeDay.Portal.Services;

public sealed record ApiMutation(HttpStatusCode Status, string? Error, int? Id)
{
    public bool Successful => (int)Status is >= 200 and < 300;
}

public sealed partial class RaceDayApiClient
{
    private static Dictionary<string, string> Cookies(HttpRequest request)
    {
        var cookies = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in request.Headers.Cookie.ToString().Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var part = item.Trim();
            var index = part.IndexOf('=');
            if (index > 0) cookies[part[..index]] = part[(index + 1)..];
        }
        return cookies;
    }
    private static void Combine(Dictionary<string, string> current, HttpResponseMessage source)
    {
        if (source.Headers.TryGetValues("Set-Cookie", out var values))
            foreach (var value in values)
            {
                var part = value.Split(';', 2)[0].Trim();
                var index = part.IndexOf('=');
                if (index > 0) current[part[..index]] = part[(index + 1)..];
            }
    }

    public async Task<ApiMutation> MutateAsync<T>(
        HttpContext browser, HttpMethod verb, string path, T? input, CancellationToken ct,
        IFormFile? image = null)
    {
        if (verb != HttpMethod.Post && verb != HttpMethod.Put && verb != HttpMethod.Delete)
            throw new ArgumentException("Mutations must use POST, PUT or DELETE.");

        // CSRF token must come from the API and be paired with its issuing cookie.
        using var tokenRequest = new HttpRequestMessage(HttpMethod.Get, "api/auth/antiforgery");
        ForwardCookies(browser.Request, tokenRequest);
        using var tokenResult = await client.SendAsync(tokenRequest, ct);
        if (!tokenResult.IsSuccessStatusCode)
            return new(tokenResult.StatusCode, "Could not validate your session. Sign in again.", null);
        using var json = await JsonDocument.ParseAsync(await tokenResult.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
        var token = json.RootElement.GetProperty("requestToken").GetString();
        if (string.IsNullOrWhiteSpace(token)) return new(HttpStatusCode.BadRequest, "Missing API security token.", null);

        using var request = new HttpRequestMessage(verb, path);
        request.Headers.TryAddWithoutValidation("X-CSRF-TOKEN", token);
        var cookies = Cookies(browser.Request);
        Combine(cookies, tokenResult);
        if (cookies.Count > 0)
            request.Headers.TryAddWithoutValidation("Cookie", string.Join("; ", cookies.Select(x => x.Key + "=" + x.Value)));

        if (image is not null)
        {
            var multipart = new MultipartFormDataContent();
            var stream = image.OpenReadStream();
            var upload = new StreamContent(stream);
            upload.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/octet-stream");
            multipart.Add(upload, "file", "upload.bin");
            request.Content = multipart;
        }
        else if (verb != HttpMethod.Delete)
            request.Content = JsonContent.Create(input, options: Json);

        RelayCookies(browser.Response, tokenResult);
        using var result = await client.SendAsync(request, ct);
        RelayCookies(browser.Response, result);
        var body = await result.Content.ReadAsStringAsync(ct);
        int? id = null;
        string? error = null;
        if (body.Length > 0 && body.Length < 32_768)
        {
            try
            {
                using var resultJson = JsonDocument.Parse(body);
                var root = resultJson.RootElement;
                if (root.ValueKind == JsonValueKind.Object)
                {
                    if (root.TryGetProperty("id", out var itemId) && itemId.ValueKind == JsonValueKind.Number && itemId.TryGetInt32(out var parsed))
                        id = parsed;
                    if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                        error = message.GetString();
                    else if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                        error = title.GetString();
                }
            }
            catch (JsonException) { }
        }
        if (error?.Length > 250) error = error[..250];
        return new(result.StatusCode, error, id);
    }

    public async Task<(byte[]? Bytes, string Type, HttpStatusCode Status)> ImageAsync(
        string path, HttpRequest browser, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        ForwardCookies(browser, request);
        using var result = await client.SendAsync(request, ct);
        if (!result.IsSuccessStatusCode) return (null, "", result.StatusCode);
        if (result.Content.Headers.ContentLength > 3 * 1024 * 1024) return (null, "", HttpStatusCode.RequestEntityTooLarge);
        var bytes = await result.Content.ReadAsByteArrayAsync(ct);
        if (bytes.Length > 3 * 1024 * 1024) return (null, "", HttpStatusCode.RequestEntityTooLarge);
        var type = result.Content.Headers.ContentType?.MediaType;
        return (bytes, type is "image/jpeg" or "image/png" ? type : "application/octet-stream", result.StatusCode);
    }
}