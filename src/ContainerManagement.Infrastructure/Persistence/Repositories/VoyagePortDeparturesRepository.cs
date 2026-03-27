using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Voyages;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories;

public class VoyagePortDeparturesRepository : IVoyagePortDeparturesRepository
{
    private readonly AppDbContext _db;

    public VoyagePortDeparturesRepository(AppDbContext db) => _db = db;

    public async Task<VoyagePortDeparture?> GetByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default)
    {
        var e = await _db.Set<VoyagePortDepartureEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.VoyagePortId == voyagePortId && !x.IsDeleted, ct);
        return e == null ? null : ToDomain(e);
    }

    public async Task<VoyagePortDeparture?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var e = await _db.Set<VoyagePortDepartureEntity>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        return e == null ? null : ToDomain(e);
    }

    public async Task AddAsync(VoyagePortDeparture departure, CancellationToken ct = default)
    {
        _db.Set<VoyagePortDepartureEntity>().Add(ToEntity(departure));
        await _db.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(VoyagePortDeparture departure, CancellationToken ct = default)
    {
        var entity = await _db.Set<VoyagePortDepartureEntity>()
            .FirstOrDefaultAsync(x => x.Id == departure.Id && !x.IsDeleted, ct);
        if (entity == null) return;

        entity.InboundVoyage = departure.InboundVoyage;
        entity.OutboundVoyage = departure.OutboundVoyage;
        entity.CompleteCargoOperation = departure.CompleteCargoOperation;
        entity.PilotOnBoard = departure.PilotOnBoard;
        entity.UnberthFAOP = departure.UnberthFAOP;
        entity.ActualETD = departure.ActualETD;
        entity.NextPortId = departure.NextPortId;
        entity.ETANextPort = departure.ETANextPort;
        entity.TugsOut = departure.TugsOut;
        entity.DepDraftFwdMtr = departure.DepDraftFwdMtr;
        entity.DepDraftAftMtr = departure.DepDraftAftMtr;
        entity.DepDraftMeanMtr = departure.DepDraftMeanMtr;
        entity.FuelOil = departure.FuelOil;
        entity.DieselOil = departure.DieselOil;
        entity.FreshWater = departure.FreshWater;
        entity.BallastWater = departure.BallastWater;
        entity.Remarks = departure.Remarks;
        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedBy = departure.ModifiedBy;

        await _db.SaveChangesAsync(ct);
    }

    public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
    {
        var entity = await _db.Set<VoyagePortDepartureEntity>()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity == null) return;

        entity.IsDeleted = true;
        entity.ModifiedOn = DateTime.UtcNow;
        entity.ModifiedBy = modifiedBy;
        await _db.SaveChangesAsync(ct);
    }

    private static VoyagePortDeparture ToDomain(VoyagePortDepartureEntity e) => new()
    {
        Id = e.Id,
        VoyagePortId = e.VoyagePortId,
        InboundVoyage = e.InboundVoyage,
        OutboundVoyage = e.OutboundVoyage,
        CompleteCargoOperation = e.CompleteCargoOperation,
        PilotOnBoard = e.PilotOnBoard,
        UnberthFAOP = e.UnberthFAOP,
        ActualETD = e.ActualETD,
        NextPortId = e.NextPortId,
        ETANextPort = e.ETANextPort,
        TugsOut = e.TugsOut,
        DepDraftFwdMtr = e.DepDraftFwdMtr,
        DepDraftAftMtr = e.DepDraftAftMtr,
        DepDraftMeanMtr = e.DepDraftMeanMtr,
        FuelOil = e.FuelOil,
        DieselOil = e.DieselOil,
        FreshWater = e.FreshWater,
        BallastWater = e.BallastWater,
        Remarks = e.Remarks,
        IsDeleted = e.IsDeleted,
        CreatedOn = e.CreatedOn,
        ModifiedOn = e.ModifiedOn,
        CreatedBy = e.CreatedBy,
        ModifiedBy = e.ModifiedBy
    };

    private static VoyagePortDepartureEntity ToEntity(VoyagePortDeparture d)
    {
        var now = DateTime.UtcNow;
        return new()
        {
            Id = d.Id == Guid.Empty ? Guid.NewGuid() : d.Id,
            VoyagePortId = d.VoyagePortId,
            InboundVoyage = d.InboundVoyage,
            OutboundVoyage = d.OutboundVoyage,
            CompleteCargoOperation = d.CompleteCargoOperation,
            PilotOnBoard = d.PilotOnBoard,
            UnberthFAOP = d.UnberthFAOP,
            ActualETD = d.ActualETD,
            NextPortId = d.NextPortId,
            ETANextPort = d.ETANextPort,
            TugsOut = d.TugsOut,
            DepDraftFwdMtr = d.DepDraftFwdMtr,
            DepDraftAftMtr = d.DepDraftAftMtr,
            DepDraftMeanMtr = d.DepDraftMeanMtr,
            FuelOil = d.FuelOil,
            DieselOil = d.DieselOil,
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
