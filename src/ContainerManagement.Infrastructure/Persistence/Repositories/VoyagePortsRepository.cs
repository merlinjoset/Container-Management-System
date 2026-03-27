using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class VoyagePortsRepository : IVoyagePortsRepository
{
    private readonly AppDbContext _db;

    public VoyagePortsRepository(AppDbContext db) => _db = db;

    public async Task<List<VoyagePort>> GetByVoyageIdAsync(Guid voyageId, CancellationToken ct = default)
    {
        return await _db.Set<VoyagePortEntity>()
            .Where(x => x.VoyageId == voyageId && !x.IsDeleted)
            .OrderBy(x => x.SortOrder)
            .Select(x => ToDomain(x))
            .ToListAsync(ct);
    }

    public async Task AddAsync(VoyagePort port, CancellationToken ct = default)
    {
        _db.Set<VoyagePortEntity>().Add(ToEntity(port));
        await _db.SaveChangesAsync(ct);
    }

    public async Task AddRangeAsync(IEnumerable<VoyagePort> ports, CancellationToken ct = default)
    {
        _db.Set<VoyagePortEntity>().AddRange(ports.Select(ToEntity));
        await _db.SaveChangesAsync(ct);
    }

    public async Task DeleteByVoyageIdAsync(Guid voyageId, CancellationToken ct = default)
    {
        var entities = await _db.Set<VoyagePortEntity>()
            .Where(x => x.VoyageId == voyageId)
            .ToListAsync(ct);

        if (entities.Any())
        {
            _db.Set<VoyagePortEntity>().RemoveRange(entities);
            await _db.SaveChangesAsync(ct);
        }
    }

    public async Task UpdateETDAsync(Guid voyagePortId, DateTime etd, Guid modifiedBy, CancellationToken ct = default)
    {
        var entity = await _db.Set<VoyagePortEntity>()
            .FirstOrDefaultAsync(x => x.Id == voyagePortId && !x.IsDeleted, ct);
        if (entity == null) return;

        entity.ETD = etd;
        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedBy = modifiedBy;
        await _db.SaveChangesAsync(ct);
    }

    private static VoyagePort ToDomain(VoyagePortEntity e) => new()
    {
        Id = e.Id,
        VoyageId = e.VoyageId,
        VoyNo = e.VoyNo,
        Bound = e.Bound,
        PortId = e.PortId,
        TerminalId = e.TerminalId,
        ETA = e.ETA,
        ETB = e.ETB,
        ETD = e.ETD,
        PortStay = e.PortStay,
        SeaDay = e.SeaDay,
        Speed = e.Speed,
        Distance = e.Distance,
        SortOrder = e.SortOrder,
        IsDeleted = e.IsDeleted,
        CreatedOn = e.CreatedOn,
        ModifiedOn = e.ModifiedOn,
        CreatedBy = e.CreatedBy,
        ModifiedBy = e.ModifiedBy
    };

    private static VoyagePortEntity ToEntity(VoyagePort d) => new()
    {
        Id = d.Id,
        VoyageId = d.VoyageId,
        VoyNo = d.VoyNo,
        Bound = d.Bound,
        PortId = d.PortId,
        TerminalId = d.TerminalId,
        ETA = d.ETA,
        ETB = d.ETB,
        ETD = d.ETD,
        PortStay = d.PortStay,
        SeaDay = d.SeaDay,
        Speed = d.Speed,
        Distance = d.Distance,
        SortOrder = d.SortOrder,
        IsDeleted = d.IsDeleted,
        CreatedOn = d.CreatedOn,
        ModifiedOn = d.ModifiedOn,
        CreatedBy = d.CreatedBy,
        ModifiedBy = d.ModifiedBy
    };
}
