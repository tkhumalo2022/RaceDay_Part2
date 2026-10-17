using System.ComponentModel.DataAnnotations;

namespace RaCeDay.Models;

// A record of a payment attempt, NOT evidence that money was received.
// Status can only be changed by future trusted server-side reconciliation.
public class PaymentAttempt
{
    public int Id { get; set; }
    public int EnrolmentId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = "EFT";
    public string Status { get; set; } = "Pending";
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? TransactionReference { get; set; }
}

public class PaymentInput
{
    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Amount { get; set; }

    [Required, RegularExpression("^(Card|EFT|Cash|Other)$", ErrorMessage = "Choose Card, EFT, Cash or Other.")]
    public string PaymentMethod { get; set; } = "EFT";

    [StringLength(80)]
    public string? TransactionReference { get; set; }
}

public record PaymentDto(int Id, int EnrolmentId, decimal Amount, string PaymentMethod, string Status, DateTime PaymentDate, string? TransactionReference);
