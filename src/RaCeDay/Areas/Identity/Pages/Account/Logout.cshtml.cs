using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RaCeDay.Models;
using RaCeDay.Services;

namespace RaCeDay.Areas.Identity.Pages.Account;

[Authorize]
public class LogoutModel(SignInManager<ApplicationUser> signIn) : PageModel
{
    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        SessionIdentity.Clear(HttpContext);
        await signIn.SignOutAsync();
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl : Url.Content("~/"));
    }
}
