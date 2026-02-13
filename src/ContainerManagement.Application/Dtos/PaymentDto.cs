using ContainerManagement.Domain.Payments;

namespace ContainerManagement.Application.Dtos;

public class PaymentDto
{
    public Guid Id { get; set; }
    public string ProviderName { get; set; } = default!;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = default!;
    public PaymentStatus Status { get; set; }
    public int AttemptCount { get; set; }
    public DateTime CreationTimeUtc { get; set; }
    public string? LastError { get; set; }
}
