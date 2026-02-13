using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos;
using ContainerManagement.Domain.Payments;

namespace ContainerManagement.Application.Services;

public class PaymentService
{
    private readonly IPaymentRepository _payments;
    private readonly IProviderRepository _providers;
    private readonly IUnitOfWork _uow;
    private const int MaxAttempts = 3;

    public PaymentService(IPaymentRepository payments, IProviderRepository providers, IUnitOfWork uow)
    {
        _payments = payments;
        _providers = providers;
        _uow = uow;
    }

    public Task<Guid> CreateAsync(CreatePaymentRequestDto dto, Guid userId, Func<Guid, Task> enqueueJob, CancellationToken ct)
        => _uow.ExecuteAsync(async () =>
        {
            var provider = await _providers.GetAsync(dto.ProviderId, ct) ?? throw new InvalidOperationException("Provider not found.");
            if (!provider.IsActive) throw new InvalidOperationException("Provider inactive.");

            // derive card last4 (do not persist full PAN or CVC)
            var digits = new string((dto.CardNumber ?? string.Empty).Where(char.IsDigit).ToArray());
            var last4 = digits.Length >= 4 ? digits[^4..] : null;

            var p = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                ProviderId = provider.Id,
                Amount = dto.Amount,
                Currency = dto.Currency.ToUpperInvariant(),
                Reference = dto.Reference.Trim(),
                Status = PaymentStatus.Pending,
                CreationTimeUtc = DateTime.UtcNow,
                CardLast4 = last4,
                CardExpMonth = dto.ExpiryMonth,
                CardExpYear = dto.ExpiryYear
            };

            await _payments.AddAsync(p, ct);
            await _payments.SaveChangesAsync(ct);

            await enqueueJob(p.Id);
            return p.Id;
        }, ct);

    public Task<List<Payment>> GetListAsync(Guid userId, bool isAdmin, CancellationToken ct)
        => _payments.GetListAsync(userId, isAdmin, ct);

    public async Task RetryAsync(Guid paymentId, bool isAdmin, Func<Guid, Task> enqueue, CancellationToken ct)
    {
        if (!isAdmin) throw new UnauthorizedAccessException("Only admins can retry.");

        var p = await _payments.GetAsync(paymentId, ct) ?? throw new InvalidOperationException("Payment not found.");

        if (p.Status != PaymentStatus.Failed)
            throw new InvalidOperationException("Only failed payments can be retried.");

        if (p.AttemptCount >= MaxAttempts)
            throw new InvalidOperationException("Max attempts reached. Cannot retry.");

        await enqueue(paymentId);
    }
    public Task<List<ContainerManagement.Domain.Providers.PaymentProvider>> GetProvidersAsync(CancellationToken ct)
    => _providers.GetListAsync(ct);

}
