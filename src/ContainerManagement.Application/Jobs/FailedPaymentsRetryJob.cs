using ContainerManagement.Application.Abstractions;

namespace ContainerManagement.Application.Jobs;

public class FailedPaymentsRetryJob
{
    private readonly IPaymentRepository _payments;

    public FailedPaymentsRetryJob(IPaymentRepository payments) => _payments = payments;

    public async Task<List<Guid>> GetRetryIdsAsync(CancellationToken ct)
        => await _payments.GetFailedForRetryAsync(maxAttempts: 3, take: 50, ct);
}
