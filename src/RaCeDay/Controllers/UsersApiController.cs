using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Models;

namespace RaCeDay.Controllers;

// Profile endpoints from the student's Part 1 plan.
// Only an allowlisted edit model is accepted. Identity owns role, email and account ID.
[ApiController, Route("api/users"), Authorize, AutoValidateAntiforgeryToken]
public class UsersApiController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn) : ControllerBase
{
    private static UserProfileDto ViewUser(ApplicationUser u) =>
        new(u.Id, u.Email, u.FirstName, u.LastName, u.Gender, u.PhoneNumber);

    /// <summary>Get the signed-in user profile.</summary>
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Me()
    {
        var user = await users.GetUserAsync(User);
        return user is null ? Unauthorized() : Ok(ViewUser(user));
    }

    /// <summary>Edit only the current user profile fields, preserving roles and email.</summary>
    [HttpPut("me")]
    public async Task<ActionResult<UserProfileDto>> Update(ProfileViewModel input)
    {
        var user = await users.GetUserAsync(User);
        if (user is null) return Unauthorized();

        user.FirstName = input.FirstName.Trim();
        user.LastName = input.LastName.Trim();
        user.Gender = string.IsNullOrWhiteSpace(input.Gender) ? null : input.Gender.Trim();
        var phone = string.IsNullOrWhiteSpace(input.PhoneNumber) ? null : input.PhoneNumber.Trim();
        if (phone != user.PhoneNumber) user.PhoneNumberConfirmed = false;
        user.PhoneNumber = phone;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors.Select(e => e.Description) });

        await signIn.RefreshSignInAsync(user);
        return Ok(ViewUser(user));
    }
}

public record UserProfileDto(string Id, string? Email, string FirstName, string LastName, string? Gender, string? PhoneNumber);
