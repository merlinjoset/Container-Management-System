using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos;

public class CreatePaymentRequestDto : IValidatableObject
{
    [Required] public Guid ProviderId { get; set; }
    [Range(0.01, 1000000)] public decimal Amount { get; set; }
    [Required] public string? Currency { get; set; }
    public string? Reference { get; set; }

    // Card details (collected but only last4 + expiry stored)
    [Required]
    [RegularExpression(@"^\d{13,19}$", ErrorMessage = "Card number must be 13-19 digits.")]
    public string? CardNumber { get; set; }

    [Range(1,12)]
    public int ExpiryMonth { get; set; }

    [Range(2020, 2100)]
    public int ExpiryYear { get; set; }

    [Required]
    [RegularExpression(@"^\d{3}$", ErrorMessage = "CVV must be exactly 3 digits.")]
    public string? Cvv { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ExpiryMonth < 1 || ExpiryMonth > 12)
        {
            yield return new ValidationResult("Expiry month must be between 1 and 12.", new[] { nameof(ExpiryMonth) });
        }

        var now = DateTime.UtcNow;
        var month = Math.Clamp(ExpiryMonth, 1, 12);
        bool parsed = true;
        DateTime expiryEnd = now;
        try
        {
            var lastDay = DateTime.DaysInMonth(ExpiryYear, month);
            expiryEnd = new DateTime(ExpiryYear, month, lastDay, 23, 59, 59, DateTimeKind.Utc);
        }
        catch { parsed = false; }

        if (!parsed)
        {
            yield return new ValidationResult("Invalid expiry date.", new[] { nameof(ExpiryMonth), nameof(ExpiryYear) });
        }
        else if (expiryEnd < now)
        {
            yield return new ValidationResult("Card has expired.", new[] { nameof(ExpiryMonth), nameof(ExpiryYear) });
        }

    }
}
