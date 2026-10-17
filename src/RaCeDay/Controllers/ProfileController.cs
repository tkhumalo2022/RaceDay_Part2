using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Models;

namespace RaCeDay.Controllers;

[Authorize]
public class ProfileController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
        return View(new ProfileViewModel
        {
            FirstName = user.FirstName, LastName = user.LastName,
            Gender = user.Gender, PhoneNumber = user.PhoneNumber,
            OrganiserName = user.OrganiserName, DateOfBirth = user.DateOfBirth
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(ProfileViewModel model)
    {
        if (!ModelState.IsValid) return View(model);
        var user = await users.GetUserAsync(User);
        if (user is null) return Challenge();
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
}
