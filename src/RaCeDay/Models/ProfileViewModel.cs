using System.ComponentModel.DataAnnotations;

namespace RaCeDay.Models;

public class ProfileViewModel
{
    [Required, StringLength(60), Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(60), Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;
    [StringLength(30)]
    public string? Gender { get; set; }
    [Phone, StringLength(30), Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }
}
