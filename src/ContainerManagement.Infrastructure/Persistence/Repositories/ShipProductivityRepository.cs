using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class ShipProductivityRepository : IShipProductivityRepository
{
    private readonly AppDbContext _db;

    public ShipProductivityRepository(AppDbContext db) => _db = db;

    public async Task<ShipProductivity?> GetByTosIdAsync(Guid tosId, CancellationToken ct = default)
    {
        var e = await _db.Set<ShipProductivityEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.TosId == tosId && !x.IsDeleted, ct);
        return e == null ? null : ToDomain(e);
    }

    public async Task UpsertAsync(ShipProductivity sp, CancellationToken ct = default)
    {
        var existing = await _db.Set<ShipProductivityEntity>()
            .FirstOrDefaultAsync(x => x.TosId == sp.TosId && !x.IsDeleted, ct);

        var now = DateTime.UtcNow;
        if (existing == null)
        {
            _db.Set<ShipProductivityEntity>().Add(new ShipProductivityEntity
            {
                Id = Guid.NewGuid(),
                TosId = sp.TosId,
                PortStayTime = sp.PortStayTime,
                ProductivityPerHr = sp.ProductivityPerHr,
                MovesPerCrane = sp.MovesPerCrane,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = sp.CreatedBy,
                ModifiedBy = sp.CreatedBy
            });
        }
        else
        {
            existing.PortStayTime = sp.PortStayTime;
            existing.ProductivityPerHr = sp.ProductivityPerHr;
            existing.MovesPerCrane = sp.MovesPerCrane;
            existing.ModifiedOn = now;
            existing.ModifiedBy = sp.ModifiedBy;
        }

        await _db.SaveChangesAsync(ct);
    }

    private static ShipProductivity ToDomain(ShipProductivityEntity e) => new()
    {
        Id = e.Id,
        TosId = e.TosId,
        PortStayTime = e.PortStayTime,
        ProductivityPerHr = e.ProductivityPerHr,
        MovesPerCrane = e.MovesPerCrane,
        IsDeleted = e.IsDeleted,
        CreatedOn = e.CreatedOn,
        ModifiedOn = e.ModifiedOn,
        CreatedBy = e.CreatedBy,
        ModifiedBy = e.ModifiedBy
    };
}
