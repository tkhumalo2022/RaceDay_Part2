using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel(
    UserManager<ApplicationUser> users,
    SignInManager<ApplicationUser> signIn,
    ILogger<RegisterModel> logger) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public class InputModel : IValidatableObject
    {
        [Required, StringLength(60), Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(60), Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Gender { get; set; }

        [StringLength(100), Display(Name = "Organisation name")]
        public string? OrganiserName { get; set; }

        [DataType(DataType.Date), Display(Name = "Date of birth")]
        public DateOnly? DateOfBirth { get; set; }

        [Phone, StringLength(20), Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            if (Role == "Organiser" && string.IsNullOrWhiteSpace(OrganiserName))
                yield return new("Organisers must provide an organisation name.", [nameof(OrganiserName)]);
            if (Role == "Participant")
            {
                if (DateOfBirth is null || DateOfBirth < new DateOnly(1900, 1, 1) ||
                    DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
                    yield return new("Participants must enter a valid date of birth.", [nameof(DateOfBirth)]);
            }
        }

        // The official POE permits selecting either application role.
        // Admin is never offered or accepted during self-registration.
        [Required, RegularExpression("^(Participant|Organiser)$",
            ErrorMessage = "Choose Participant or Organiser."), Display(Name = "Register as")]
        public string Role { get; set; } = "Participant";

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100, MinimumLength = 6), DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password), Display(Name = "Confirm password")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null) =>
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
        if (!ModelState.IsValid)
            return Page();

        var user = new ApplicationUser
        {
            UserName = Input.Email.Trim(),
            Email = Input.Email.Trim(),
            FirstName = Input.FirstName.Trim(),
            LastName = Input.LastName.Trim(),
            Gender = string.IsNullOrWhiteSpace(Input.Gender) ? null : Input.Gender.Trim(),
            OrganiserName = Input.Role == "Organiser" ? Input.OrganiserName?.Trim() : null,
            DateOfBirth = Input.Role == "Participant" ? Input.DateOfBirth : null,
            PhoneNumber = string.IsNullOrWhiteSpace(Input.PhoneNumber) ? null : Input.PhoneNumber.Trim()
        };

        var created = await users.CreateAsync(user, Input.Password);
        if (!created.Succeeded)
        {
            foreach (var error in created.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        // Only the explicitly allowlisted application role can be self-selected.
        // Public callers can never create or assign the Admin role.
        var assigned = await users.AddToRoleAsync(user, Input.Role);
        if (!assigned.Succeeded)
        {
            await users.DeleteAsync(user);
            foreach (var error in assigned.Errors)
                ModelState.AddModelError(string.Empty, error.Description);
            return Page();
        }

        logger.LogInformation("A {RegistrationRole} account was registered.", Input.Role);
        if (users.Options.SignIn.RequireConfirmedAccount)
            return RedirectToPage("RegisterConfirmation", new { email = user.Email, returnUrl = ReturnUrl });

        await signIn.SignInAsync(user, isPersistent: false);
        await SessionIdentity.RememberAsync(HttpContext, users, user);
        return LocalRedirect(ReturnUrl!);
    }
}
