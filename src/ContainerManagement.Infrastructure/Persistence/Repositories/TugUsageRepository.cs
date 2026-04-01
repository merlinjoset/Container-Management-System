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
        // Deduplicate by TugNumber — keep last
        var deduped = tugs
            .GroupBy(t => t.TugNumber)
            .Select(g => g.Last())
            .ToList();

        var existing = await _db.Set<TugUsageEntity>()
            .Where(x => x.ArrivalId == arrivalId && !x.IsDeleted)
            .ToListAsync(ct);

        var existingByNum = existing.ToDictionary(e => e.TugNumber);
        var incomingNums = new HashSet<int>(deduped.Select(t => t.TugNumber));
        var now = DateTime.UtcNow;

        // Soft-delete rows no longer in input
        foreach (var e in existing.Where(e => !incomingNums.Contains(e.TugNumber)))
        {
            e.IsDeleted = true;
            e.ModifiedOn = now;
        }

        // Upsert
        foreach (var t in deduped)
        {
            if (existingByNum.TryGetValue(t.TugNumber, out var entity))
            {
                // Update only if changed
                if (entity.Hours != t.Hours)
                {
                    entity.Hours = t.Hours;
                    entity.ModifiedOn = now;
                    entity.ModifiedBy = t.CreatedBy;
                }
            }
            else
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
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task ReplaceForDepartureAsync(Guid departureId, List<TugUsage> tugs, CancellationToken ct = default)
    {
        // Deduplicate by TugNumber — keep last
        var deduped = tugs
            .GroupBy(t => t.TugNumber)
            .Select(g => g.Last())
            .ToList();

        var existing = await _db.Set<TugUsageEntity>()
            .Where(x => x.DepartureId == departureId && !x.IsDeleted)
            .ToListAsync(ct);

        var existingByNum = existing.ToDictionary(e => e.TugNumber);
        var incomingNums = new HashSet<int>(deduped.Select(t => t.TugNumber));
        var now = DateTime.UtcNow;

        // Soft-delete rows no longer in input
        foreach (var e in existing.Where(e => !incomingNums.Contains(e.TugNumber)))
        {
            e.IsDeleted = true;
            e.ModifiedOn = now;
        }

        // Upsert
        foreach (var t in deduped)
        {
            if (existingByNum.TryGetValue(t.TugNumber, out var entity))
            {
                if (entity.Hours != t.Hours)
                {
                    entity.Hours = t.Hours;
                    entity.ModifiedOn = now;
                    entity.ModifiedBy = t.CreatedBy;
                }
            }
            else
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
