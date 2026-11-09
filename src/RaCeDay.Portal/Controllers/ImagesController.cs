using Microsoft.AspNetCore.Mvc;
using RaCeDay.Portal.Services;

namespace RaCeDay.Portal.Controllers;
public sealed class ImagesController(RaceDayApiClient api) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Event(int id, CancellationToken ct)
    {
        if (id <= 0) return NotFound();
        try
        {
            var (bytes, type, _) = await api.ImageAsync(
                $"api/events/{id}/banner", Request, ct);
            if (bytes is null) return NotFound();
            Response.Headers.CacheControl = "private, max-age=60";
            return File(bytes, type);
        }
        catch (HttpRequestException) { return NotFound(); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested) { return NotFound(); }
    }
    [HttpGet]
    public async Task<IActionResult> Profile(CancellationToken ct)
    {
        try
        {
            var (bytes, type, _) = await api.ImageAsync(
                "api/users/me/picture", Request, ct);
            if (bytes is null) return NotFound();
            Response.Headers.CacheControl = "private, no-store";
            return File(bytes, type);
        }
        catch (HttpRequestException) { return NotFound(); }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested) { return NotFound(); }
    }
}