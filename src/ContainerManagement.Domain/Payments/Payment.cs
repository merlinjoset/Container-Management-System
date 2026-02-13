using ContainerManagement.Application;
using ContainerManagement.Domain.Providers;

namespace ContainerManagement.Domain.Payments;

public class Payment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }

    public Guid ProviderId { get; set; }
    public PaymentProvider? Provider { get; set; }

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Reference { get; set; } = default!;

    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public int AttemptCount { get; set; } = 0;

    public DateTime CreationTimeUtc { get; set; } = DateTime.UtcNow;
    public DateTime? LastTriedAtUtc { get; set; }
    public string? LastError { get; set; }
    public string? CardLast4 { get; set; }
    public int? CardExpMonth { get; set; }
    public int? CardExpYear { get; set; }
}
