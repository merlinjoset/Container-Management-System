namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class PaymentEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }
    public UserEntity? User { get; set; }

    public Guid ProviderId { get; set; }
    public PaymentProviderEntity? Provider { get; set; }

    public decimal Amount { get; set; }
    public string? Currency { get; set; }
    public string? Reference { get; set; }

    public int Status { get; set; } 
    public int AttemptCount { get; set; }

    public DateTime CreationTimeUtc { get; set; }
    public DateTime? LastTriedAtUtc { get; set; }
    public string? LastError { get; set; }

    // Card (summary only)
    public string? CardLast4 { get; set; }
    public int? CardExpMonth { get; set; }
    public int? CardExpYear { get; set; }
}
