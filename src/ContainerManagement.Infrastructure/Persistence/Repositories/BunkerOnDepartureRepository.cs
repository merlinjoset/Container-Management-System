using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class BunkerOnDepartureRepository : IBunkerOnDepartureRepository
{
    private readonly AppDbContext _db;

    public BunkerOnDepartureRepository(AppDbContext db) => _db = db;

    public async Task<List<BunkerOnDeparture>> GetByDepartureIdAsync(Guid departureId, CancellationToken ct = default)
    {
        return await _db.Set<BunkerOnDepartureEntity>()
            .AsNoTracking()
            .Where(x => x.DepartureId == departureId && !x.IsDeleted)
            .OrderBy(x => x.ReadingPoint)
            .Select(x => new BunkerOnDeparture
            {
                Id = x.Id,
                DepartureId = x.DepartureId,
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

    public async Task ReplaceForDepartureAsync(Guid departureId, List<BunkerOnDeparture> bunkers, CancellationToken ct = default)
    {
        var deduped = bunkers
            .GroupBy(b => b.ReadingPoint)
            .Select(g => g.Last())
            .ToList();

        var existing = await _db.Set<BunkerOnDepartureEntity>()
            .Where(x => x.DepartureId == departureId && !x.IsDeleted)
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
                _db.Set<BunkerOnDepartureEntity>().Add(new BunkerOnDepartureEntity
                {
                    Id = Guid.NewGuid(),
                    DepartureId = departureId,
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
