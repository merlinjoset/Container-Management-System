using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class VoyagePortArrivalsRepository : IVoyagePortArrivalsRepository
{
    private readonly AppDbContext _db;

    public VoyagePortArrivalsRepository(AppDbContext db) => _db = db;

    public async Task<VoyagePortArrival?> GetByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default)
    {
        var e = await _db.Set<VoyagePortArrivalEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VoyagePortId == voyagePortId && !x.IsDeleted, ct);
        return e == null ? null : ToDomain(e);
    }

    public async Task<VoyagePortArrival?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Set<VoyagePortArrivalEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return e == null ? null : ToDomain(e);
    }

    public async Task AddAsync(VoyagePortArrival arrival, CancellationToken ct = default)
    {
        _db.Set<VoyagePortArrivalEntity>().Add(ToEntity(arrival));
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(VoyagePortArrival arrival, CancellationToken ct = default)
    {
        var entity = await _db.Set<VoyagePortArrivalEntity>()
            .FirstOrDefaultAsync(x => x.Id == arrival.Id && !x.IsDeleted, ct);
        if (entity == null) return;

        entity.InboundVoyage = arrival.InboundVoyage;
        entity.OutboundVoyage = arrival.OutboundVoyage;
        entity.ActualETA = arrival.ActualETA;
        entity.ActualETB = arrival.ActualETB;
        entity.LastPortId = arrival.LastPortId;
        entity.NextPortId = arrival.NextPortId;
        entity.PilotOnBoard = arrival.PilotOnBoard;
        entity.CommencedCargoOperation = arrival.CommencedCargoOperation;
        entity.TugsIn = arrival.TugsIn;
        entity.ArrivalDraftFwdMtr = arrival.ArrivalDraftFwdMtr;
        entity.ArrivalDraftAftMtr = arrival.ArrivalDraftAftMtr;
        entity.ArrivalDraftMeanMtr = arrival.ArrivalDraftMeanMtr;
        entity.FreshWater = arrival.FreshWater;
        entity.BallastWater = arrival.BallastWater;
        entity.Remarks = arrival.Remarks;
        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedBy = arrival.ModifiedBy;

        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
    {
        var entity = await _db.Set<VoyagePortArrivalEntity>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity == null) return;

        entity.IsDeleted = true;
        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedBy = modifiedBy;
        await _db.SaveChangesAsync(ct);
    }

    private static VoyagePortArrival ToDomain(VoyagePortArrivalEntity e) => new()
    {
        Id = e.Id,
        VoyagePortId = e.VoyagePortId,
        InboundVoyage = e.InboundVoyage,
        OutboundVoyage = e.OutboundVoyage,
        ActualETA = e.ActualETA,
        ActualETB = e.ActualETB,
        LastPortId = e.LastPortId,
        NextPortId = e.NextPortId,
        PilotOnBoard = e.PilotOnBoard,
        CommencedCargoOperation = e.CommencedCargoOperation,
        TugsIn = e.TugsIn,
        ArrivalDraftFwdMtr = e.ArrivalDraftFwdMtr,
        ArrivalDraftAftMtr = e.ArrivalDraftAftMtr,
        ArrivalDraftMeanMtr = e.ArrivalDraftMeanMtr,
        FreshWater = e.FreshWater,
        BallastWater = e.BallastWater,
        Remarks = e.Remarks,
        IsDeleted = e.IsDeleted,
        CreatedOn = e.CreatedOn,
        ModifiedOn = e.ModifiedOn,
        CreatedBy = e.CreatedBy,
        ModifiedBy = e.ModifiedBy
    };

    private static VoyagePortArrivalEntity ToEntity(VoyagePortArrival d)
    {
        var now = DateTime.UtcNow;
        return new()
        {
            Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
            VoyagePortId = d.VoyagePortId,
            InboundVoyage = d.InboundVoyage,
            OutboundVoyage = d.OutboundVoyage,
            ActualETA = d.ActualETA,
            ActualETB = d.ActualETB,
            LastPortId = d.LastPortId,
            NextPortId = d.NextPortId,
            PilotOnBoard = d.PilotOnBoard,
            CommencedCargoOperation = d.CommencedCargoOperation,
            TugsIn = d.TugsIn,
            ArrivalDraftFwdMtr = d.ArrivalDraftFwdMtr,
            ArrivalDraftAftMtr = d.ArrivalDraftAftMtr,
            ArrivalDraftMeanMtr = d.ArrivalDraftMeanMtr,
            FreshWater = d.FreshWater,
            BallastWater = d.BallastWater,
            Remarks = d.Remarks,
            IsDeleted = false,
            CreatedOn = now,
            ModifiedOn = now,
            CreatedBy = d.CreatedBy,
            ModifiedBy = d.CreatedBy
        };
    }
}
