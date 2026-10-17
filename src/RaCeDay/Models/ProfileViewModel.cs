using System.ComponentModel.DataAnnotations;

namespace RaCeDay.Models;

public class ProfileViewModel : IValidatableObject
{
    [Required, StringLength(60), Display(Name = "First name")]
    public string FirstName { get; set; } = string.Empty;
    [Required, StringLength(60), Display(Name = "Last name")]
    public string LastName { get; set; } = string.Empty;
    [StringLength(30), RegularExpression("^(Male|Female|Other|Prefer not to say)$", ErrorMessage = "Select a permitted gender option.")]
    public string? Gender { get; set; }
    [Phone, StringLength(30), Display(Name = "Phone number")]
    public string? PhoneNumber { get; set; }

    [StringLength(100), Display(Name = "Organisation name")]
    public string? OrganiserName { get; set; }

    [DataType(DataType.Date), Display(Name = "Date of birth")]
    public DateOnly? DateOfBirth { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DateOfBirth is DateOnly dob &&
            (dob < new DateOnly(1900, 1, 1) || dob > DateOnly.FromDateTime(DateTime.UtcNow)))
            yield return new("Date of birth must be a valid past date.", [nameof(DateOfBirth)]);
    }
}
