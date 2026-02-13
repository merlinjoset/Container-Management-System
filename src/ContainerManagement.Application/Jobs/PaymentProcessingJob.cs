using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Payments;

namespace ContainerManagement.Application.Jobs;

public class PaymentProcessingJob
{
    private readonly IPaymentRepository _payments;
    private readonly IProviderRepository _providers;
    private readonly IFakeProviderClient _client;
    private readonly IUnitOfWork _uow;
    private readonly IPaymentEvents _events;

    public PaymentProcessingJob(IPaymentRepository payments, IProviderRepository providers, IFakeProviderClient client, IUnitOfWork uow, IPaymentEvents events)
    {
        _payments = payments;
        _providers = providers;
        _client = client;
        _uow = uow;
        _events = events;
    }

    public Task ProcessAsync(Guid paymentId, CancellationToken ct)
        => _uow.ExecuteAsync(async () =>
        {
            // Domain read (no Infrastructure types)
            var p = await _payments.GetAsync(paymentId, ct);
            if (p is null || p.Status == PaymentStatus.Completed) return;

            // attempt + processing
            await _payments.IncrementAttemptAsync(paymentId, ct);

            var attempts = p.AttemptCount + 1; // because p is a snapshot before increment
            if (attempts > 3)
            {
                await _payments.MarkFailedAsync(paymentId, "Max attempts reached.", ct);
                await _payments.SaveChangesAsync(ct);
                return;
            }

            await _payments.MarkProcessingAsync(paymentId, ct);
            await _payments.SaveChangesAsync(ct);

            try
            {
                _ = await _providers.GetAsync(p.ProviderId, ct) ?? throw new Exception("Provider missing.");
                var res = await _client.PayAsync(p.Id, p.Amount, p.Currency, p.Reference, ct);

                if (res.Success)
                    await _payments.MarkCompletedAsync(paymentId, ct);
                else
                    await _payments.MarkFailedAsync(paymentId, res.Error ?? "Provider failure.", ct);
            }
            catch (Exception ex)
            {
                await _payments.MarkFailedAsync(paymentId, ex.Message, ct);
            }

            await _payments.SaveChangesAsync(ct);
            await _events.NotifyPaymentUpdatedAsync(paymentId, p.UserId, ct);
        }, ct);
}
