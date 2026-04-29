using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class EditRequestRepository : IEditRequestRepository
{
    private readonly AppDbContext _db;

    public EditRequestRepository(AppDbContext db) => _db = db;

    public async Task<Guid> AddAsync(EditRequest request, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var entity = new EditRequestEntity
        {
            Id = Guid.NewGuid(),
            VoyagePortId = request.VoyagePortId,
            ReportType = request.ReportType,
            Reason = request.Reason,
            Status = string.IsNullOrWhiteSpace(request.Status) ? "Pending" : request.Status,
            ApprovedBy = request.ApprovedBy,
            ApprovedOn = request.ApprovedOn,
            IsDeleted = false,
            CreatedOn = now,
            ModifiedOn = now,
            CreatedBy = request.CreatedBy,
            ModifiedBy = request.CreatedBy
        };
        _db.Set<EditRequestEntity>().Add(entity);
        await _db.SaveChangesAsync(ct);
        return entity.Id;
    }

    public async Task<List<EditRequest>> GetPendingAsync(CancellationToken ct = default)
    {
        return await _db.Set<EditRequestEntity>()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Status == "Pending")
            .OrderByDescending(x => x.CreatedOn)
            .Select(x => ToDomain(x))
            .ToListAsync(ct);
    }

    public async Task<EditRequest?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Set<EditRequestEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return e == null ? null : ToDomain(e);
    }

    public async Task UpdateStatusAsync(Guid id, string status, Guid approvedBy, CancellationToken ct = default)
    {
        var entity = await _db.Set<EditRequestEntity>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity == null) return;
        entity.Status = status;
        entity.ApprovedBy = approvedBy;
        entity.ApprovedOn = DateTime.UtcNow;
        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedBy = approvedBy;
        await _db.SaveChangesAsync(ct);
    }

    private static EditRequest ToDomain(EditRequestEntity e) => new()
    {
        Id = e.Id,
        VoyagePortId = e.VoyagePortId,
        ReportType = e.ReportType,
        Reason = e.Reason,
        Status = e.Status,
        ApprovedBy = e.ApprovedBy,
        ApprovedOn = e.ApprovedOn,
        IsDeleted = e.IsDeleted,
        CreatedOn = e.CreatedOn,
        ModifiedOn = e.ModifiedOn,
        CreatedBy = e.CreatedBy,
        ModifiedBy = e.ModifiedBy
    };
}
