using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class TosRepository : ITosRepository
{
    private readonly AppDbContext _db;

    public TosRepository(AppDbContext db) => _db = db;

    public async Task<Tos?> GetByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default)
    {
        var e = await _db.Set<TosEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VoyagePortId == voyagePortId && !x.IsDeleted, ct);
        return e == null ? null : ToDomain(e);
    }

    public async Task AddAsync(Tos tos, CancellationToken ct = default)
    {
        _db.Set<TosEntity>().Add(ToEntity(tos));
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Tos tos, CancellationToken ct = default)
    {
        var entity = await _db.Set<TosEntity>()
            .FirstOrDefaultAsync(x => x.Id == tos.Id && !x.IsDeleted, ct);
        if (entity == null) return;

        entity.DischargeMoves = tos.DischargeMoves;
        entity.LoadMoves = tos.LoadMoves;
        entity.Restows = tos.Restows;
        entity.TotalHatchCover = tos.TotalHatchCover;
        entity.NoOfBin = tos.NoOfBin;
        entity.TotalMoves = tos.TotalMoves;
        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedBy = tos.ModifiedBy;

        await _db.SaveChangesAsync(ct);
    }

    private static Tos ToDomain(TosEntity e) => new()
    {
        Id = e.Id,
        VoyagePortId = e.VoyagePortId,
        DischargeMoves = e.DischargeMoves,
        LoadMoves = e.LoadMoves,
        Restows = e.Restows,
        TotalHatchCover = e.TotalHatchCover,
        NoOfBin = e.NoOfBin,
        TotalMoves = e.TotalMoves,
        IsDeleted = e.IsDeleted,
        CreatedOn = e.CreatedOn,
        ModifiedOn = e.ModifiedOn,
        CreatedBy = e.CreatedBy,
        ModifiedBy = e.ModifiedBy
    };

    private static TosEntity ToEntity(Tos d)
    {
        var now = DateTime.UtcNow;
        return new()
        {
            Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
            VoyagePortId = d.VoyagePortId,
            DischargeMoves = d.DischargeMoves,
            LoadMoves = d.LoadMoves,
            Restows = d.Restows,
            TotalHatchCover = d.TotalHatchCover,
            NoOfBin = d.NoOfBin,
            TotalMoves = d.TotalMoves,
            IsDeleted = false,
            CreatedOn = now,
            ModifiedOn = now,
            CreatedBy = d.CreatedBy,
            ModifiedBy = d.CreatedBy
        };
    }
}
