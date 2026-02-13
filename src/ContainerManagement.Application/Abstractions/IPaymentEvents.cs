namespace ContainerManagement.Application.Abstractions;

public interface IPaymentEvents
{
    Task NotifyPaymentUpdatedAsync(Guid paymentId, Guid userId, CancellationToken ct);
}
