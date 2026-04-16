using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class CraneProductivityRepository : ICraneProductivityRepository
{
    private readonly AppDbContext _db;

    public CraneProductivityRepository(AppDbContext db) => _db = db;

    public async Task<CraneProductivity?> GetByTosIdAsync(Guid tosId, CancellationToken ct = default)
    {
        var e = await _db.Set<CraneProductivityEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TosId == tosId && !x.IsDeleted, ct);
        return e == null ? null : ToDomain(e);
    }

    public async Task UpsertAsync(CraneProductivity cp, CancellationToken ct = default)
    {
        var existing = await _db.Set<CraneProductivityEntity>()
            .FirstOrDefaultAsync(x => x.TosId == cp.TosId && !x.IsDeleted, ct);

        var now = DateTime.UtcNow;
        if (existing == null)
        {
            _db.Set<CraneProductivityEntity>().Add(new CraneProductivityEntity
            {
                Id = Guid.NewGuid(),
                TosId = cp.TosId,
                CranesPerSI = cp.CranesPerSI,
                ActualCranes = cp.ActualCranes,
                TotalOpsTime = cp.TotalOpsTime,
                NonProductiveTime = cp.NonProductiveTime,
                CraneWorkingHrs = cp.CraneWorkingHrs,
                CraneProductivityPerHr = cp.CraneProductivityPerHr,
                MovesPerCrane = cp.MovesPerCrane,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = cp.CreatedBy,
                ModifiedBy = cp.CreatedBy
            });
        }
        else
        {
            existing.CranesPerSI = cp.CranesPerSI;
            existing.ActualCranes = cp.ActualCranes;
            existing.TotalOpsTime = cp.TotalOpsTime;
            existing.NonProductiveTime = cp.NonProductiveTime;
            existing.CraneWorkingHrs = cp.CraneWorkingHrs;
            existing.CraneProductivityPerHr = cp.CraneProductivityPerHr;
            existing.MovesPerCrane = cp.MovesPerCrane;
            existing.ModifiedOn = now;
            existing.ModifiedBy = cp.ModifiedBy;
        }

        await _db.SaveChangesAsync(ct);
    }

    private static CraneProductivity ToDomain(CraneProductivityEntity e) => new()
    {
        Id = e.Id,
        TosId = e.TosId,
        CranesPerSI = e.CranesPerSI,
        ActualCranes = e.ActualCranes,
        TotalOpsTime = e.TotalOpsTime,
        NonProductiveTime = e.NonProductiveTime,
        CraneWorkingHrs = e.CraneWorkingHrs,
        CraneProductivityPerHr = e.CraneProductivityPerHr,
        MovesPerCrane = e.MovesPerCrane,
        IsDeleted = e.IsDeleted,
        CreatedOn = e.CreatedOn,
        ModifiedOn = e.ModifiedOn,
        CreatedBy = e.CreatedBy,
        ModifiedBy = e.ModifiedBy
    };
}
