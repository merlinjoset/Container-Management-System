using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class TosStoppageRepository : ITosStoppageRepository
{
    private readonly AppDbContext _db;

    public TosStoppageRepository(AppDbContext db) => _db = db;

    public async Task<List<TosStoppage>> GetByTosIdAsync(Guid tosId, CancellationToken ct = default)
    {
        return await _db.Set<TosStoppageEntity>()
            .AsNoTracking()
            .Where(x => x.TosId == tosId && !x.IsDeleted)
            .OrderBy(x => x.RowNumber)
            .Select(x => ToDomain(x))
            .ToListAsync(ct);
    }

    public async Task ReplaceForTosAsync(Guid tosId, List<TosStoppage> stoppages, CancellationToken ct = default)
    {
        // Deduplicate by RowNumber — keep last
        var deduped = stoppages
            .GroupBy(s => s.RowNumber)
            .Select(g => g.Last())
            .ToList();

        var existing = await _db.Set<TosStoppageEntity>()
            .Where(x => x.TosId == tosId && !x.IsDeleted)
            .ToListAsync(ct);

        var existingByNum = existing.ToDictionary(e => e.RowNumber);
        var incomingNums = new HashSet<int>(deduped.Select(s => s.RowNumber));
        var now = DateTime.UtcNow;

        // Soft-delete rows no longer in input
        foreach (var e in existing.Where(e => !incomingNums.Contains(e.RowNumber)))
        {
            e.IsDeleted = true;
            e.ModifiedOn = now;
        }

        // Upsert
        foreach (var s in deduped)
        {
            if (existingByNum.TryGetValue(s.RowNumber, out var entity))
            {
                // Update only if changed
                if (entity.FromDateTime != s.FromDateTime
                    || entity.ToDateTime != s.ToDateTime
                    || entity.NonProductiveHours != s.NonProductiveHours
                    || entity.Reason != s.Reason)
                {
                    entity.FromDateTime = s.FromDateTime;
                    entity.ToDateTime = s.ToDateTime;
                    entity.NonProductiveHours = s.NonProductiveHours;
                    entity.Reason = s.Reason;
                    entity.ModifiedOn = now;
                    entity.ModifiedBy = s.CreatedBy;
                }
            }
            else
            {
                _db.Set<TosStoppageEntity>().Add(new TosStoppageEntity
                {
                    Id = Guid.NewGuid(),
                    TosId = tosId,
                    RowNumber = s.RowNumber,
                    FromDateTime = s.FromDateTime,
                    ToDateTime = s.ToDateTime,
                    NonProductiveHours = s.NonProductiveHours,
                    Reason = s.Reason,
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

    private static TosStoppage ToDomain(TosStoppageEntity x) => new()
    {
        Id = x.Id,
        TosId = x.TosId,
        RowNumber = x.RowNumber,
        FromDateTime = x.FromDateTime,
        ToDateTime = x.ToDateTime,
        NonProductiveHours = x.NonProductiveHours,
        Reason = x.Reason,
        IsDeleted = x.IsDeleted,
        CreatedOn = x.CreatedOn,
        ModifiedOn = x.ModifiedOn,
        CreatedBy = x.CreatedBy,
        ModifiedBy = x.ModifiedBy
    };
}
