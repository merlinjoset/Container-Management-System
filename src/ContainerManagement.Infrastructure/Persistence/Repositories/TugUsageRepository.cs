using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class TugUsageRepository : ITugUsageRepository
{
    private readonly AppDbContext _db;

    public TugUsageRepository(AppDbContext db) => _db = db;

    public async Task<List<TugUsage>> GetByArrivalIdAsync(Guid arrivalId, CancellationToken ct = default)
    {
        return await _db.Set<TugUsageEntity>()
            .AsNoTracking()
            .Where(x => x.ArrivalId == arrivalId && !x.IsDeleted)
            .OrderBy(x => x.TugNumber)
            .Select(x => ToDomain(x))
            .ToListAsync(ct);
    }

    public async Task<List<TugUsage>> GetByDepartureIdAsync(Guid departureId, CancellationToken ct = default)
    {
        return await _db.Set<TugUsageEntity>()
            .AsNoTracking()
            .Where(x => x.DepartureId == departureId && !x.IsDeleted)
            .OrderBy(x => x.TugNumber)
            .Select(x => ToDomain(x))
            .ToListAsync(ct);
    }

    public async Task ReplaceForArrivalAsync(Guid arrivalId, List<TugUsage> tugs, CancellationToken ct = default)
    {
        // Soft-delete existing
        var existing = await _db.Set<TugUsageEntity>()
            .Where(x => x.ArrivalId == arrivalId && !x.IsDeleted)
            .ToListAsync(ct);
        foreach (var e in existing)
        {
            e.IsDeleted = true;
            e.ModifiedOn = DateTime.UtcNow;
        }

        // Add new rows
        var now = DateTime.UtcNow;
        foreach (var t in tugs)
        {
            _db.Set<TugUsageEntity>().Add(new TugUsageEntity
            {
                Id = Guid.NewGuid(),
                ArrivalId = arrivalId,
                DepartureId = null,
                TugNumber = t.TugNumber,
                Hours = t.Hours,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = t.CreatedBy,
                ModifiedBy = t.CreatedBy
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ReplaceForDepartureAsync(Guid departureId, List<TugUsage> tugs, CancellationToken ct = default)
    {
        var existing = await _db.Set<TugUsageEntity>()
            .Where(x => x.DepartureId == departureId && !x.IsDeleted)
            .ToListAsync(ct);
        foreach (var e in existing)
        {
            e.IsDeleted = true;
            e.ModifiedOn = DateTime.UtcNow;
        }

        var now = DateTime.UtcNow;
        foreach (var t in tugs)
        {
            _db.Set<TugUsageEntity>().Add(new TugUsageEntity
            {
                Id = Guid.NewGuid(),
                ArrivalId = null,
                DepartureId = departureId,
                TugNumber = t.TugNumber,
                Hours = t.Hours,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = t.CreatedBy,
                ModifiedBy = t.CreatedBy
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    private static TugUsage ToDomain(TugUsageEntity x) => new()
    {
        Id = x.Id,
        ArrivalId = x.ArrivalId,
        DepartureId = x.DepartureId,
        TugNumber = x.TugNumber,
        Hours = x.Hours,
        IsDeleted = x.IsDeleted,
        CreatedOn = x.CreatedOn,
        ModifiedOn = x.ModifiedOn,
        CreatedBy = x.CreatedBy,
        ModifiedBy = x.ModifiedBy
    };
}
