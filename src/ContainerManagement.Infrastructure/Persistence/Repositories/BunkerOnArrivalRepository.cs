using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class BunkerOnArrivalRepository : IBunkerOnArrivalRepository
{
    private readonly AppDbContext _db;

    public BunkerOnArrivalRepository(AppDbContext db) => _db = db;

    public async Task<List<BunkerOnArrival>> GetByArrivalIdAsync(Guid arrivalId, CancellationToken ct = default)
    {
        return await _db.Set<BunkerOnArrivalEntity>()
            .AsNoTracking()
            .Where(x => x.ArrivalId == arrivalId && !x.IsDeleted)
            .OrderBy(x => x.ReadingPoint)
            .Select(x => new BunkerOnArrival
            {
                Id = x.Id,
                ArrivalId = x.ArrivalId,
                ReadingPoint = x.ReadingPoint,
                VlsfoMts = x.VlsfoMts,
                MgoMts = x.MgoMts,
                HfoMts = x.HfoMts,
                CreatedOn = x.CreatedOn,
                ModifiedOn = x.ModifiedOn,
                CreatedBy = x.CreatedBy,
                ModifiedBy = x.ModifiedBy,
                IsDeleted = x.IsDeleted
            })
            .ToListAsync(ct);
    }

    public async Task ReplaceForArrivalAsync(Guid arrivalId, List<BunkerOnArrival> bunkers, CancellationToken ct = default)
    {
        var deduped = bunkers
            .GroupBy(b => b.ReadingPoint)
            .Select(g => g.Last())
            .ToList();

        var existing = await _db.Set<BunkerOnArrivalEntity>()
            .Where(x => x.ArrivalId == arrivalId && !x.IsDeleted)
            .ToListAsync(ct);

        var existingByPt = existing.ToDictionary(e => e.ReadingPoint);
        var incomingPts = new HashSet<string>(deduped.Select(b => b.ReadingPoint));
        var now = DateTime.UtcNow;

        foreach (var e in existing.Where(e => !incomingPts.Contains(e.ReadingPoint)))
        {
            e.IsDeleted = true;
            e.ModifiedOn = now;
        }

        foreach (var b in deduped)
        {
            if (existingByPt.TryGetValue(b.ReadingPoint, out var entity))
            {
                if (entity.VlsfoMts != b.VlsfoMts || entity.MgoMts != b.MgoMts || entity.HfoMts != b.HfoMts)
                {
                    entity.VlsfoMts = b.VlsfoMts;
                    entity.MgoMts = b.MgoMts;
                    entity.HfoMts = b.HfoMts;
                    entity.ModifiedOn = now;
                    entity.ModifiedBy = b.CreatedBy;
                }
            }
            else
            {
                _db.Set<BunkerOnArrivalEntity>().Add(new BunkerOnArrivalEntity
                {
                    Id = Guid.NewGuid(),
                    ArrivalId = arrivalId,
                    ReadingPoint = b.ReadingPoint,
                    VlsfoMts = b.VlsfoMts,
                    MgoMts = b.MgoMts,
                    HfoMts = b.HfoMts,
                    IsDeleted = false,
                    CreatedOn = now,
                    ModifiedOn = now,
                    CreatedBy = b.CreatedBy,
                    ModifiedBy = b.CreatedBy
                });
            }
        }

        await _db.SaveChangesAsync(ct);
    }
}
