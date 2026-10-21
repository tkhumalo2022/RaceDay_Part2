using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users) : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; private set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public void OnGet(string? returnUrl = null) =>
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/");
        if (!ModelState.IsValid)
            return Page();

        var result = await signIn.PasswordSignInAsync(
            Input.Email.Trim(), Input.Password, Input.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            var user = await users.FindByEmailAsync(Input.Email.Trim())
                ?? throw new InvalidOperationException("Signed-in user disappeared.");
            await SessionIdentity.RememberAsync(HttpContext, users, user);
            return LocalRedirect(ReturnUrl!);
        }

        if (result.RequiresTwoFactor)
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl, Input.RememberMe });

        ModelState.AddModelError(string.Empty, result.IsLockedOut
            ? "Account temporarily locked. Please try again later."
            : "Invalid login attempt.");
        return Page();
    }
}
