using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RaCeDay.Models;

namespace RaCeDay.Controllers;

public sealed class ApiRegisterInput : IValidatableObject
{
    [Required, StringLength(60)] public string FirstName { get; set; } = "";
    [Required, StringLength(60)] public string LastName { get; set; } = "";
    [StringLength(30)] public string? Gender { get; set; }
    [StringLength(100)] public string? OrganiserName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [Phone, StringLength(20)] public string? PhoneNumber { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (Role == "Organiser" && string.IsNullOrWhiteSpace(OrganiserName))
            yield return new("Organisers must provide an organisation name.", [nameof(OrganiserName)]);
        if (Role == "Participant" &&
            (DateOfBirth is null || DateOfBirth < new DateOnly(1900, 1, 1) ||
             DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow)))
            yield return new("Participants must provide a valid date of birth.", [nameof(DateOfBirth)]);
    }
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, RegularExpression("^(Participant|Organiser)$")]
    public string Role { get; set; } = "Participant";
    [Required, StringLength(100, MinimumLength = 6)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
}

public sealed class ApiLoginInput
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}

// API authentication uses the same Identity accounts and secure cookie session as MVC.
// Admin is NEVER eligible for self-registration. All state-changing operations use CSRF.
[ApiController, Route("api/auth"), AutoValidateAntiforgeryToken]
public class AuthApiController(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn) : ControllerBase
{
    /// <summary>Get a CSRF request token before registering, logging in, or writing via the API.</summary>
    [HttpGet("antiforgery"), AllowAnonymous]
    public IActionResult Antiforgery([FromServices] IAntiforgery antiforgery) =>
        Ok(new { requestToken = antiforgery.GetAndStoreTokens(HttpContext).RequestToken });

    /// <summary>Register as Organiser or Participant; public Admin registration is forbidden.</summary>
    [HttpPost("register"), AllowAnonymous]
    [ProducesResponseType(201)]
    public async Task<IActionResult> Register(ApiRegisterInput input)
    {
        var email = input.Email.Trim();
        var user = new ApplicationUser
        {
            UserName = email, Email = email,
            FirstName = input.FirstName.Trim(), LastName = input.LastName.Trim(),
            Gender = string.IsNullOrWhiteSpace(input.Gender) ? null : input.Gender.Trim(),
            OrganiserName = input.Role == "Organiser" ? input.OrganiserName?.Trim() : null,
            DateOfBirth = input.Role == "Participant" ? input.DateOfBirth : null,
            PhoneNumber = string.IsNullOrWhiteSpace(input.PhoneNumber) ? null : input.PhoneNumber.Trim()
        };
        var created = await users.CreateAsync(user, input.Password);
        if (!created.Succeeded)
        {
            var errors = created.Errors.Select(e => e.Description).ToArray();
            if (created.Errors.Any(e => e.Code is "DuplicateEmail" or "DuplicateUserName"))
                return Conflict(new { errors });
            return BadRequest(new { errors });
        }
        var assigned = await users.AddToRoleAsync(user, input.Role);
        if (!assigned.Succeeded)
        {
            await users.DeleteAsync(user);
            return BadRequest(new { errors = assigned.Errors.Select(e => e.Description).ToArray() });
        }

        var confirmationRequired = users.Options.SignIn.RequireConfirmedAccount;
        if (!confirmationRequired)
            await signIn.SignInAsync(user, isPersistent: false);
        return Created("/api/users/me", new
        {
            userId = user.Id, role = input.Role,
            status = confirmationRequired ? "ConfirmationRequired" : "SignedIn"
        });
    }

    /// <summary>Log in using the secure Identity cookie session, not a JWT.</summary>
    [HttpPost("login"), AllowAnonymous]
    public async Task<IActionResult> Login(ApiLoginInput input)
    {
        var user = await users.FindByEmailAsync(input.Email.Trim());
        if (user is null) return Unauthorized(new { message = "Invalid login attempt." });

        var result = await signIn.PasswordSignInAsync(user, input.Password, isPersistent: false, lockoutOnFailure: true);
        if (result.IsLockedOut) return StatusCode(423, new { message = "Account temporarily locked." });
        if (result.RequiresTwoFactor) return StatusCode(403, new { message = "Two-factor sign-in is required." });
        if (result.IsNotAllowed) return StatusCode(403, new { message = "Confirm your account before signing in." });
        if (!result.Succeeded) return Unauthorized(new { message = "Invalid login attempt." });

        return Ok(new { userId = user.Id, roles = await users.GetRolesAsync(user) });
    }

    /// <summary>Sign out and end the current cookie session.</summary>
    [HttpPost("logout"), Authorize]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return NoContent();
    }
}
