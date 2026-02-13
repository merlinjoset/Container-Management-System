using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Providers;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class EfProviderRepository : IProviderRepository
{
    private readonly AppDbContext _db;
    public EfProviderRepository(AppDbContext db) => _db = db;

    public async Task<PaymentProvider?> GetAsync(Guid id, CancellationToken ct)
    {
        var e = await _db.PaymentProviders
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        return e == null ? null : ToDomain(e);
    }

    public async Task<List<PaymentProvider>> GetListAsync(CancellationToken ct)
    {
        var list = await _db.PaymentProviders
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(ct);

        return list.Select(ToDomain).ToList();
    }

    public Task AddAsync(PaymentProvider provider, CancellationToken ct)
    {
        _db.PaymentProviders.Add(ToEntity(provider));
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(PaymentProvider provider, CancellationToken ct)
    {
        var e = await _db.PaymentProviders.FirstOrDefaultAsync(x => x.Id == provider.Id, ct);
        if (e == null) throw new InvalidOperationException("Provider not found.");

        e.Name = provider.Name;
        e.EndpointUrl = provider.EndpointUrl;
        e.IsActive = provider.IsActive;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var e = await _db.PaymentProviders.FirstOrDefaultAsync(x => x.Id == id, ct);
        if (e == null) return;

        _db.PaymentProviders.Remove(e);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);

    // -------- mapping --------
    private static PaymentProvider ToDomain(PaymentProviderEntity e) => new()
    {
        Id = e.Id,
        Name = e.Name,
        IsActive = e.IsActive,
        EndpointUrl = e.EndpointUrl
    };

    private static PaymentProviderEntity ToEntity(PaymentProvider d) => new()
    {
        Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
        Name = d.Name,
        IsActive = d.IsActive,
        EndpointUrl = d.EndpointUrl
    };
}
