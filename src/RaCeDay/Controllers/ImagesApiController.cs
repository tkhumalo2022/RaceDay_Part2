using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaCeDay.Data;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Controllers;

// All image handling runs through Part 2 API. Container is private and unauthenticated
// clients cannot read other users' profiles or change someone else's event banner.
[ApiController, Route("api"), Authorize, AutoValidateAntiforgeryToken]
public sealed class ImagesApiController(
    RaceWorkflow workflow, AzureRaceImageStore images,
    UserManager<ApplicationUser> users, ApplicationDbContext db) : ControllerBase
{
    [HttpPost("events/{id:int}/banner"), Authorize(Roles = "Organiser"),
     Consumes("multipart/form-data"), RequestSizeLimit(4_194_304)]
    public async Task<IActionResult> EventBanner(int id, IFormFile file, CancellationToken ct)
    {
        var item = await workflow.Event(User, id);
        if (!item.IsOwner) return Forbid();
        try
        {
            var blobUrl = await images.SaveAsync(AzureRaceImageStore.EventKey(id), file, ct);
            var race = await db.Events.SingleAsync(e => e.Id == id, ct);
            race.BannerBlobUrl = blobUrl;
            await db.SaveChangesAsync(ct);
            return Ok(new { path = $"/api/events/{id}/banner", blobUrl });
        }
        catch (InvalidRaceImageException e) { return BadRequest(new { message = e.Message }); }
        catch (ImageStorageUnavailableException) { return StatusCode(503, new { message = "Image storage unavailable." }); }
    }

    [HttpGet("events/{id:int}/banner")]
    public async Task<IActionResult> EventBanner(int id, CancellationToken ct)
    {
        await workflow.Event(User, id); // Verify the referenced event exists.
        try
        {
            var image = await images.ReadAsync(AzureRaceImageStore.EventKey(id), ct);
            if (image is null) return NotFound();
            Response.Headers.CacheControl = "private, max-age=60";
            return File(image.Value.Bytes, image.Value.Type);
        }
        catch (ImageStorageUnavailableException) { return StatusCode(503); }
    }

    [HttpPost("users/me/picture"), Consumes("multipart/form-data"),
     RequestSizeLimit(4_194_304)]
    public async Task<IActionResult> ProfilePicture(IFormFile file, CancellationToken ct)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Unauthorized();
        try
        {
            var blobUrl = await images.SaveAsync(AzureRaceImageStore.ProfileKey(user.Id), file, ct);
            user.ProfileBlobUrl = blobUrl;
            var updated = await users.UpdateAsync(user);
            if (!updated.Succeeded)
                return StatusCode(500, new { message = "Picture uploaded but its database link could not be saved." });
            return Ok(new { path = "/api/users/me/picture", blobUrl });
        }
        catch (InvalidRaceImageException e) { return BadRequest(new { message = e.Message }); }
        catch (ImageStorageUnavailableException) { return StatusCode(503, new { message = "Image storage unavailable." }); }
    }

    [HttpGet("users/me/picture")]
    public async Task<IActionResult> ProfilePicture(CancellationToken ct)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Unauthorized();
        try
        {
            var image = await images.ReadAsync(AzureRaceImageStore.ProfileKey(user.Id), ct);
            if (image is null) return NotFound();
            Response.Headers.CacheControl = "private, no-store";
            return File(image.Value.Bytes, image.Value.Type);
        }
        catch (ImageStorageUnavailableException) { return StatusCode(503); }
    }
}