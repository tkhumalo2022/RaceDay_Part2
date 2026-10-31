using System.ComponentModel.DataAnnotations;
namespace RaCeDay.Portal.Models;
public sealed record RaceEvent(int Id, string EventName, string? Description,
    DateOnly EventDate, string Location, string EventType, string Status,
    DateOnly ClosingDate, bool IsOwner);
public sealed record RaceCategory(int Id, int EventId, string CategoryName,
    decimal DistanceKm, decimal EntryFee, int MaxParticipants, int CurrentParticipants);
public sealed record ApiSession(string UserId, string Role);
public sealed record EventListModel(IReadOnlyList<RaceEvent> Events, string? Role,
    bool SignInRequired, string? Error, string? Filter);
public sealed record EventPageModel(RaceEvent Event, IReadOnlyList<RaceCategory> Categories);
public sealed class LoginInput
{
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required] public string Password { get; set; } = "";
}
public sealed class RegisterInput : IValidatableObject
{
    [Required, StringLength(60)] public string FirstName { get; set; } = "";
    [Required, StringLength(60)] public string LastName { get; set; } = "";
    [Required, EmailAddress] public string Email { get; set; } = "";
    [Required, RegularExpression("^(Participant|Organiser)$")] public string Role { get; set; } = "Participant";
    [Required, MinLength(6)] public string Password { get; set; } = "";
    [Required, Compare(nameof(Password))] public string ConfirmPassword { get; set; } = "";
    public string? OrganiserName { get; set; }
    public DateOnly? DateOfBirth { get; set; }
    [Phone] public string? PhoneNumber { get; set; }
    public IEnumerable<ValidationResult> Validate(ValidationContext c)
    {
        if (Role == "Organiser" && string.IsNullOrWhiteSpace(OrganiserName))
            yield return new("Enter an organisation name.", [nameof(OrganiserName)]);
        if (Role == "Participant" && (DateOfBirth is null || DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow)))
            yield return new("Enter a valid date of birth.", [nameof(DateOfBirth)]);
    }
}
