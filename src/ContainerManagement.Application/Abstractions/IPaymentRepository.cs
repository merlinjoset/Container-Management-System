using ContainerManagement.Domain.Payments;


namespace ContainerManagement.Application.Abstractions;

public interface IPaymentRepository
{
    Task<Payment?> GetAsync(Guid id, CancellationToken ct);
    Task<List<Payment>> GetListAsync(Guid userId, bool isAdmin, CancellationToken ct);
    Task<List<Guid>> GetFailedForRetryAsync(int maxAttempts, int take, CancellationToken ct);
    Task AddAsync(Payment payment, CancellationToken ct);
    Task MarkProcessingAsync(Guid paymentId, CancellationToken ct);
    Task MarkCompletedAsync(Guid paymentId, CancellationToken ct);
    Task MarkFailedAsync(Guid paymentId, string error, CancellationToken ct);
    Task IncrementAttemptAsync(Guid paymentId, CancellationToken ct);

    Task SaveChangesAsync(CancellationToken ct);

}
