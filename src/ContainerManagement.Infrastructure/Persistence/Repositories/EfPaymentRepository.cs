using ContainerManagement.Application;
using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Payments;
using ContainerManagement.Domain.Providers;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class EfPaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _db;
    public EfPaymentRepository(AppDbContext db) => _db = db;

    public async Task<Payment?> GetAsync(Guid id, CancellationToken ct)
    {
        var e = await _db.Payments
            .AsNoTracking()
            .Include(p => p.Provider)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return e == null ? null : ToDomain(e);
    }

    public async Task<List<Payment>> GetListAsync(Guid userId, bool isAdmin, CancellationToken ct)
    {
        var q = _db.Payments
            .AsNoTracking()
            .Include(p => p.Provider)
            .AsQueryable();

        if (!isAdmin)
            q = q.Where(p => p.UserId == userId);

        var list = await q
            .OrderByDescending(p => p.CreationTimeUtc)
            .ToListAsync(ct);

        return list.Select(ToDomain).ToList();
    }

    public Task<List<Guid>> GetFailedForRetryAsync(int maxAttempts, int take, CancellationToken ct)
        => _db.Payments
            .AsNoTracking()
            .Where(p => p.Status == (int)PaymentStatus.Failed && p.AttemptCount < maxAttempts)
            .OrderBy(p => p.LastTriedAtUtc ?? p.CreationTimeUtc)
            .Take(take)
            .Select(p => p.Id)
            .ToListAsync(ct);

    public Task AddAsync(Payment payment, CancellationToken ct)
    {
        _db.Payments.Add(ToEntity(payment));
        return Task.CompletedTask;
    }

    // Job Commands
    public async Task IncrementAttemptAsync(Guid paymentId, CancellationToken ct)
    {
        var e = await _db.Payments.FirstOrDefaultAsync(x => x.Id == paymentId, ct);
        if (e == null) return;

        e.AttemptCount += 1;
        e.LastTriedAtUtc = DateTime.UtcNow;
    }

    public async Task MarkProcessingAsync(Guid paymentId, CancellationToken ct)
    {
        var e = await _db.Payments.FirstOrDefaultAsync(x => x.Id == paymentId, ct);
        if (e == null) return;

        e.Status = (int)PaymentStatus.Processing;
        e.LastTriedAtUtc = DateTime.UtcNow;
    }

    public async Task MarkCompletedAsync(Guid paymentId, CancellationToken ct)
    {
        var e = await _db.Payments.FirstOrDefaultAsync(x => x.Id == paymentId, ct);
        if (e == null) return;

        e.Status = (int)PaymentStatus.Completed;
        e.LastError = null;
    }

    public async Task MarkFailedAsync(Guid paymentId, string error, CancellationToken ct)
    {
        var e = await _db.Payments.FirstOrDefaultAsync(x => x.Id == paymentId, ct);
        if (e == null) return;

        e.Status = (int)PaymentStatus.Failed;
        e.LastError = error;
    }

    // -----------+++++++++++++++++++------------------------------

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    // ----------------- mapping -----------------

    private static Payment ToDomain(PaymentEntity e) => new()
    {
        Id = e.Id,
        UserId = e.UserId,
        ProviderId = e.ProviderId,
        Amount = e.Amount,
        Currency = e.Currency,
        Reference = e.Reference,
        Status = (PaymentStatus)e.Status,
        AttemptCount = e.AttemptCount,
        CreationTimeUtc = e.CreationTimeUtc,
        LastTriedAtUtc = e.LastTriedAtUtc,
        LastError = e.LastError,
        CardLast4 = e.CardLast4,
        CardExpMonth = e.CardExpMonth,
        CardExpYear = e.CardExpYear,
        Provider = e.Provider == null ? null : new PaymentProvider
        {
            Id = e.Provider.Id,
            Name = e.Provider.Name,
            IsActive = e.Provider.IsActive,
            EndpointUrl = e.Provider.EndpointUrl
        }
    };

    private static PaymentEntity ToEntity(Payment d) => new()
    {
        Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
        UserId = d.UserId,
        ProviderId = d.ProviderId,
        Amount = d.Amount,
        Currency = d.Currency,
        Reference = d.Reference,
        Status = (int)d.Status,
        AttemptCount = d.AttemptCount,
        CreationTimeUtc = d.CreationTimeUtc == default ? DateTime.UtcNow : d.CreationTimeUtc,
        LastTriedAtUtc = d.LastTriedAtUtc,
        LastError = d.LastError,
        CardLast4 = d.CardLast4,
        CardExpMonth = d.CardExpMonth,
        CardExpYear = d.CardExpYear
    };
}
