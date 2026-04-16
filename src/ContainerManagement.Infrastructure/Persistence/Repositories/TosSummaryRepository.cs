using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class TosSummaryRepository : ITosSummaryRepository
{
    private readonly AppDbContext _db;

    public TosSummaryRepository(AppDbContext db) => _db = db;

    public async Task<TosSummary?> GetByTosIdAsync(Guid tosId, CancellationToken ct = default)
    {
        var e = await _db.Set<TosSummaryEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TosId == tosId && !x.IsDeleted, ct);
        return e == null ? null : ToDomain(e);
    }

    public async Task UpsertAsync(TosSummary summary, CancellationToken ct = default)
    {
        var existing = await _db.Set<TosSummaryEntity>()
            .FirstOrDefaultAsync(x => x.TosId == summary.TosId && !x.IsDeleted, ct);

        var now = DateTime.UtcNow;
        if (existing == null)
        {
            _db.Set<TosSummaryEntity>().Add(new TosSummaryEntity
            {
                Id = Guid.NewGuid(),
                TosId = summary.TosId,
                VesselTurnaroundTime = summary.VesselTurnaroundTime,
                BerthingDelay = summary.BerthingDelay,
                TotalMoves = summary.TotalMoves,
                NonProductiveTime = summary.NonProductiveTime,
                TerminalProductivity = summary.TerminalProductivity,
                ShipProductivity = summary.ShipProductivity,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = summary.CreatedBy,
                ModifiedBy = summary.CreatedBy
            });
        }
        else
        {
            existing.VesselTurnaroundTime = summary.VesselTurnaroundTime;
            existing.BerthingDelay = summary.BerthingDelay;
            existing.TotalMoves = summary.TotalMoves;
            existing.NonProductiveTime = summary.NonProductiveTime;
            existing.TerminalProductivity = summary.TerminalProductivity;
            existing.ShipProductivity = summary.ShipProductivity;
            existing.ModifiedOn = now;
            existing.ModifiedBy = summary.ModifiedBy;
        }

        await _db.SaveChangesAsync(ct);
    }

    private static TosSummary ToDomain(TosSummaryEntity e) => new()
    {
        Id = e.Id,
        TosId = e.TosId,
        VesselTurnaroundTime = e.VesselTurnaroundTime,
        BerthingDelay = e.BerthingDelay,
        TotalMoves = e.TotalMoves,
        NonProductiveTime = e.NonProductiveTime,
        TerminalProductivity = e.TerminalProductivity,
        ShipProductivity = e.ShipProductivity,
        IsDeleted = e.IsDeleted,
        CreatedOn = e.CreatedOn,
        ModifiedOn = e.ModifiedOn,
        CreatedBy = e.CreatedBy,
        ModifiedBy = e.ModifiedBy
    };
}
