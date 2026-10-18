using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Controllers;

[Authorize]
public class ProfileController(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn,
    ProfileImageStorage imageStorage) : Controller
{
    private static ProfileViewModel FromUser(ApplicationUser user) => new()
    {
        FirstName = user.FirstName,
        LastName = user.LastName,
        Gender = user.Gender,
        PhoneNumber = user.PhoneNumber,
        OrganiserName = user.OrganiserName,
        DateOfBirth = user.DateOfBirth,
        ProfileImagePath = user.ProfileImagePath
    };

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserAsync(User);
        return user is null ? Challenge() : View(FromUser(user));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        model.ProfileImagePath = user.ProfileImagePath;
        if (!ModelState.IsValid) return View(model);

        user.FirstName = model.FirstName.Trim();
        user.LastName = model.LastName.Trim();
        user.Gender = string.IsNullOrWhiteSpace(model.Gender) ? null : model.Gender.Trim();
        var phone = string.IsNullOrWhiteSpace(model.PhoneNumber) ? null : model.PhoneNumber.Trim();
        if (phone != user.PhoneNumber) user.PhoneNumberConfirmed = false;
        user.PhoneNumber = phone;
        if (User.IsInRole("Organiser") && !string.IsNullOrWhiteSpace(model.OrganiserName))
            user.OrganiserName = model.OrganiserName.Trim();
        if (User.IsInRole("Participant") && model.DateOfBirth.HasValue)
            user.DateOfBirth = model.DateOfBirth;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View(model);
        }
        await signIn.RefreshSignInAsync(user);
        TempData["StatusMessage"] = "Your profile has been saved.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(2_200_000)]
    public async Task<IActionResult> UploadImage(IFormFile? image, CancellationToken cancellationToken)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        if (image is null)
        {
            ModelState.AddModelError(string.Empty, "Choose a JPG or PNG picture.");
            return View("Index", FromUser(user));
        }

        string? newPath = null;
        try
        {
            newPath = await imageStorage.SaveAsync(image, cancellationToken);
            var previous = user.ProfileImagePath;
            user.ProfileImagePath = newPath;
            var result = await users.UpdateAsync(user);
            if (!result.Succeeded)
            {
                imageStorage.Delete(newPath);
                foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
                return View("Index", FromUser(user));
            }
            await signIn.RefreshSignInAsync(user);
            // A failed cleanup must not undo an already saved profile change.
            try { imageStorage.Delete(previous); } catch (IOException) { }
            TempData["StatusMessage"] = "Profile picture updated.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidDataException error)
        {
            ModelState.AddModelError(string.Empty, error.Message);
            return View("Index", FromUser(user));
        }
        catch (IOException)
        {
            if (newPath is not null) imageStorage.Delete(newPath);
            ModelState.AddModelError(string.Empty, "The picture could not be saved. Please try again.");
            return View("Index", FromUser(user));
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveImage()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        var previous = user.ProfileImagePath;
        user.ProfileImagePath = null;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            return View("Index", FromUser(user));
        }
        try { imageStorage.Delete(previous); } catch (IOException) { }
        TempData["StatusMessage"] = "Profile picture removed.";
        return RedirectToAction(nameof(Index));
    }
}
