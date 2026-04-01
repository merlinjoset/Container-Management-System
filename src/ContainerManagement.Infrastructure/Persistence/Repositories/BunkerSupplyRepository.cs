using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class BunkerSupplyRepository : IBunkerSupplyRepository
{
    private readonly AppDbContext _db;

    public BunkerSupplyRepository(AppDbContext db) => _db = db;

    public async Task<List<BunkerSupply>> GetByDepartureIdAsync(Guid departureId, CancellationToken ct = default)
    {
        return await _db.Set<BunkerSupplyEntity>()
            .AsNoTracking()
            .Where(x => x.DepartureId == departureId && !x.IsDeleted)
            .OrderBy(x => x.FuelType)
            .Select(x => new BunkerSupply
            {
                Id = x.Id,
                DepartureId = x.DepartureId,
                FuelType = x.FuelType,
                Qty = x.Qty,
                RateMts = x.RateMts,
                CreatedOn = x.CreatedOn,
                ModifiedOn = x.ModifiedOn,
                CreatedBy = x.CreatedBy,
                ModifiedBy = x.ModifiedBy,
                IsDeleted = x.IsDeleted
            })
            .ToListAsync(ct);
    }

    public async Task ReplaceForDepartureAsync(Guid departureId, List<BunkerSupply> supplies, CancellationToken ct = default)
    {
        var deduped = supplies
            .GroupBy(s => s.FuelType)
            .Select(g => g.Last())
            .ToList();

        var existing = await _db.Set<BunkerSupplyEntity>()
            .Where(x => x.DepartureId == departureId && !x.IsDeleted)
            .ToListAsync(ct);

        var existingByFuel = existing.ToDictionary(e => e.FuelType);
        var incomingFuels = new HashSet<string>(deduped.Select(s => s.FuelType));
        var now = DateTime.UtcNow;

        foreach (var e in existing.Where(e => !incomingFuels.Contains(e.FuelType)))
        {
            e.IsDeleted = true;
            e.ModifiedOn = now;
        }

        foreach (var s in deduped)
        {
            if (existingByFuel.TryGetValue(s.FuelType, out var entity))
            {
                if (entity.Qty != s.Qty || entity.RateMts != s.RateMts)
                {
                    entity.Qty = s.Qty;
                    entity.RateMts = s.RateMts;
                    entity.ModifiedOn = now;
                    entity.ModifiedBy = s.CreatedBy;
                }
            }
            else
            {
                _db.Set<BunkerSupplyEntity>().Add(new BunkerSupplyEntity
                {
                    Id = Guid.NewGuid(),
                    DepartureId = departureId,
                    FuelType = s.FuelType,
                    Qty = s.Qty,
                    RateMts = s.RateMts,
                    IsDeleted = false,
                    CreatedOn = now,
                    ModifiedOn = now,
                    CreatedBy = s.CreatedBy,
                    ModifiedBy = s.CreatedBy
                });
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
