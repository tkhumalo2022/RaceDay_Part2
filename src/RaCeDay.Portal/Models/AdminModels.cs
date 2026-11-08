namespace RaCeDay.Portal.Models;

// Typed, read-only API response models. The portal has no EF Core dependency.
public sealed record AdminOverview(
    int Users, int Organisers, int Participants, int Events, int Categories,
    int Enrolments, int Results, int PaymentAttempts, int PendingPaymentAttempts, int UpcomingEvents);
public sealed record AdminPage<T>(int Total, int Page, int PageSize, IReadOnlyList<T> Items);
public sealed record AdminUserRow(
    string Id, string Name, string Email, DateTime CreatedAt, string Role,
    bool Disabled, bool EmailConfirmed);
public sealed record AdminEventRow(
    int Id, string EventName, DateOnly EventDate, string Location,
    string Status, string Organiser, int Enrolments, int Results);
public sealed record AdminEnrolmentRow(
    int Id, string EventName, string Participant, string Status,
    DateTime EnrolmentDate, string RaceNumber);
public sealed record AdminDashboardPage(
    AdminOverview Overview,
    AdminPage<AdminUserRow> Users,
    AdminPage<AdminEventRow> Events,
    AdminPage<AdminEnrolmentRow> Enrolments,
    string? Query,
    string? RoleFilter,
    string? Error)
{
    public int TotalPages => Math.Max(1, (int)Math.Ceiling((double)Users.Total / Users.PageSize));
}
