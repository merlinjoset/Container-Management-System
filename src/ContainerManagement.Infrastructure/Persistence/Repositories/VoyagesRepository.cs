using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class VoyagesRepository : IVoyagesRepository
{
    private readonly AppDbContext _db;

    public VoyagesRepository(AppDbContext db) => _db = db;

    public async Task<List<Voyage>> GetAllAsync(CancellationToken ct = default)
    {
        return await _db.Set<VoyageEntity>()
            .Where(x => !x.IsDeleted)
            .OrderByDescending(x => x.CreatedOn)
            .Select(x => ToDomain(x))
            .ToListAsync(ct);
    }

    public async Task<Voyage?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var entity = await _db.Set<VoyageEntity>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return entity == null ? null : ToDomain(entity);
    }

    public async Task AddAsync(Voyage voyage, CancellationToken ct = default)
    {
        _db.Set<VoyageEntity>().Add(ToEntity(voyage));
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Voyage voyage, CancellationToken ct = default)
    {
        var entity = await _db.Set<VoyageEntity>()
            .FirstOrDefaultAsync(x => x.Id == voyage.Id, ct);
        if (entity == null) return;

        entity.VesselId = voyage.VesselId;
        entity.ServiceId = voyage.ServiceId;
        entity.OperatorId = voyage.OperatorId;
        entity.VoyageType = voyage.VoyageType;
        entity.ModifiedOn = voyage.ModifiedOn;
        entity.ModifiedBy = voyage.ModifiedBy;

        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
    {
        var entity = await _db.Set<VoyageEntity>()
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (entity == null) return;

        entity.IsDeleted = true;
        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedBy = modifiedBy;
        await _db.SaveChangesAsync(ct);
    }

    private static Voyage ToDomain(VoyageEntity e) => new()
    {
        Id = e.Id,
        VesselId = e.VesselId,
        ServiceId = e.ServiceId,
        OperatorId = e.OperatorId,
        VoyageType = e.VoyageType,
        IsDeleted = e.IsDeleted,
        CreatedOn = e.CreatedOn,
        ModifiedOn = e.ModifiedOn,
        CreatedBy = e.CreatedBy,
        ModifiedBy = e.ModifiedBy
    };

    private static VoyageEntity ToEntity(Voyage d) => new()
    {
        Id = d.Id,
        VesselId = d.VesselId,
        ServiceId = d.ServiceId,
        OperatorId = d.OperatorId,
        VoyageType = d.VoyageType,
        IsDeleted = d.IsDeleted,
        CreatedOn = d.CreatedOn,
        ModifiedOn = d.ModifiedOn,
        CreatedBy = d.CreatedBy,
        ModifiedBy = d.ModifiedBy
    };
}
