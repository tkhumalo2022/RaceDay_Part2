using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RaCeDay.Models;

// ASP.NET Core Identity owns credentials and account security.
public class ApplicationUser : IdentityUser
{
    [Required, StringLength(60)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string LastName { get; set; } = string.Empty;

    [StringLength(30)]
    public string? Gender { get; set; }

    // Do not store uploads or absolute local paths here.
    [StringLength(260)]
    public string? ProfileImagePath { get; set; }
}
