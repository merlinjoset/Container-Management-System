using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Schedules;
using ContainerManagement.Application.Dtos.Voyages;
using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Services;

public class VoyageService
{
    private readonly IVoyagesRepository _voyagesRepo;
    private readonly IVoyagePortsRepository _portsRepo;
    private readonly IVesselsRepository _vesselsRepo;
    private readonly IServicesRepository _servicesRepo;
    private readonly IOperatorsRepository _operatorsRepo;
    private readonly IPortsRepository _portsMasterRepo;
    private readonly ITerminalsRepository _terminalsRepo;
    private readonly IVoyagePortArrivalsRepository _arrivalsRepo;
    private readonly IVoyagePortDeparturesRepository _departuresRepo;
    private readonly ITugUsageRepository _tugUsageRepo;
    private readonly IBunkerOnArrivalRepository _bunkerRepo;
    private readonly IBunkerOnDepartureRepository _bunkerDepRepo;
    private readonly IBunkerSupplyRepository _bunkerSupplyRepo;
    private readonly ITosRepository _tosRepo;
    private readonly ITosStoppageRepository _tosStoppageRepo;
    private readonly ICraneProductivityRepository _craneProductivityRepo;
    private readonly IShipProductivityRepository _shipProductivityRepo;
    private readonly ITosSummaryRepository _tosSummaryRepo;

    public VoyageService(
        IVoyagesRepository voyagesRepo,
        IVoyagePortsRepository portsRepo,
        IVesselsRepository vesselsRepo,
        IServicesRepository servicesRepo,
        IOperatorsRepository operatorsRepo,
        IPortsRepository portsMasterRepo,
        ITerminalsRepository terminalsRepo,
        IVoyagePortArrivalsRepository arrivalsRepo,
        IVoyagePortDeparturesRepository departuresRepo,
        ITugUsageRepository tugUsageRepo,
        IBunkerOnArrivalRepository bunkerRepo,
        IBunkerOnDepartureRepository bunkerDepRepo,
        IBunkerSupplyRepository bunkerSupplyRepo,
        ITosRepository tosRepo,
        ITosStoppageRepository tosStoppageRepo,
        ICraneProductivityRepository craneProductivityRepo,
        IShipProductivityRepository shipProductivityRepo,
        ITosSummaryRepository tosSummaryRepo)
    {
        _voyagesRepo = voyagesRepo;
        _portsRepo = portsRepo;
        _vesselsRepo = vesselsRepo;
        _servicesRepo = servicesRepo;
        _operatorsRepo = operatorsRepo;
        _portsMasterRepo = portsMasterRepo;
        _terminalsRepo = terminalsRepo;
        _arrivalsRepo = arrivalsRepo;
        _departuresRepo = departuresRepo;
        _tugUsageRepo = tugUsageRepo;
        _bunkerRepo = bunkerRepo;
        _bunkerDepRepo = bunkerDepRepo;
        _bunkerSupplyRepo = bunkerSupplyRepo;
        _tosRepo = tosRepo;
        _tosStoppageRepo = tosStoppageRepo;
        _craneProductivityRepo = craneProductivityRepo;
        _shipProductivityRepo = shipProductivityRepo;
        _tosSummaryRepo = tosSummaryRepo;
    }

    public async Task<List<VoyageListItemDto>> GetAllAsync(CancellationToken ct = default)
    {
        var voyages = await _voyagesRepo.GetAllAsync(ct);
        var vessels = await _vesselsRepo.GetAllAsync(ct);
        var services = await _servicesRepo.GetAllAsync(ct);
        var operators = await _operatorsRepo.GetAllAsync(ct);

        var vesselLookup = vessels.ToDictionary(v => v.Id, v => v.VesselName ?? "");
        var serviceLookup = services.ToDictionary(s => s.Id);
        var operatorLookup = operators.ToDictionary(o => o.Id, o => o.OperatorName ?? "");

        return voyages.Select(v => new VoyageListItemDto
        {
            Id = v.Id,
            VesselId = v.VesselId,
            VesselName = vesselLookup.TryGetValue(v.VesselId, out var vn) ? vn : null,
            ServiceId = v.ServiceId,
            ServiceCode = v.ServiceId.HasValue && serviceLookup.TryGetValue(v.ServiceId.Value, out var svc) ? svc.ServiceCode : null,
            ServiceName = v.ServiceId.HasValue && serviceLookup.TryGetValue(v.ServiceId.Value, out var svc2) ? svc2.ServiceName : null,
            OperatorId = v.OperatorId,
            OperatorName = v.OperatorId.HasValue && operatorLookup.TryGetValue(v.OperatorId.Value, out var on) ? on : null,
            VoyageType = v.VoyageType,
            CreatedOn = v.CreatedOn
        }).ToList();
    }

    public async Task<VoyageListItemDto?> GetByIdWithPortsAsync(Guid id, CancellationToken ct = default)
    {
        var voyage = await _voyagesRepo.GetByIdAsync(id, ct);
        if (voyage == null) return null;

        var vessels = await _vesselsRepo.GetAllAsync(ct);
        var services = await _servicesRepo.GetAllAsync(ct);
        var operators = await _operatorsRepo.GetAllAsync(ct);

        var vesselLookup = vessels.ToDictionary(v => v.Id, v => v.VesselName ?? "");
        var serviceLookup = services.ToDictionary(s => s.Id);
        var operatorLookup = operators.ToDictionary(o => o.Id, o => o.OperatorName ?? "");

        return new VoyageListItemDto
        {
            Id = voyage.Id,
            VesselId = voyage.VesselId,
            VesselName = vesselLookup.TryGetValue(voyage.VesselId, out var vn) ? vn : null,
            ServiceId = voyage.ServiceId,
            ServiceCode = voyage.ServiceId.HasValue && serviceLookup.TryGetValue(voyage.ServiceId.Value, out var svc) ? svc.ServiceCode : null,
            ServiceName = voyage.ServiceId.HasValue && serviceLookup.TryGetValue(voyage.ServiceId.Value, out var svc2) ? svc2.ServiceName : null,
            OperatorId = voyage.OperatorId,
            OperatorName = voyage.OperatorId.HasValue && operatorLookup.TryGetValue(voyage.OperatorId.Value, out var on) ? on : null,
            VoyageType = voyage.VoyageType,
            CreatedOn = voyage.CreatedOn
        };
    }

    public async Task<List<VoyagePortListItemDto>> GetPortsByVoyageIdAsync(Guid voyageId, CancellationToken ct = default)
    {
        var ports = await _portsRepo.GetByVoyageIdAsync(voyageId, ct);
        var allPorts = await _portsMasterRepo.GetAllAsync(ct);
        var terminals = await _terminalsRepo.GetAllAsync(ct);

        var portLookup = allPorts.ToDictionary(p => p.Id, p => p.PortCode ?? p.FullName ?? "");
        var termLookup = terminals.ToDictionary(t => t.Id, t => t.TerminalCode ?? t.TerminalName ?? "");

        var result = new List<VoyagePortListItemDto>();
        foreach (var p in ports.OrderBy(p => p.SortOrder))
        {
            var arrival = await _arrivalsRepo.GetByVoyagePortIdAsync(p.Id, ct);
            var departure = await _departuresRepo.GetByVoyagePortIdAsync(p.Id, ct);
            var tos = await _tosRepo.GetByVoyagePortIdAsync(p.Id, ct);
            var dto = new VoyagePortListItemDto
            {
                Id = p.Id,
                VoyageId = p.VoyageId,
                VoyNo = p.VoyNo,
                Bound = p.Bound,
                PortId = p.PortId,
                PortCode = portLookup.TryGetValue(p.PortId, out var pc) ? pc : null,
                TerminalId = p.TerminalId,
                TerminalCode = p.TerminalId.HasValue && termLookup.TryGetValue(p.TerminalId.Value, out var tc) ? tc : null,
                ETA = p.ETA,
                ETB = p.ETB,
                ETD = p.ETD,
                PortStay = p.PortStay,
                SeaDay = p.SeaDay,
                Speed = p.Speed,
                Distance = p.Distance,
                SortOrder = p.SortOrder,
                HasArrival = arrival != null && arrival.ActualETA.HasValue && arrival.ActualETB.HasValue,
                HasDeparture = departure != null && departure.ActualETD.HasValue,
                HasTos = tos != null
            };

            // Populate actual dates from arrival
            if (arrival != null)
            {
                dto.ActualETA = arrival.ActualETA;
                dto.ActualETB = arrival.ActualETB;
                dto.ArrPilotOnBoard = arrival.PilotOnBoard;
                dto.ArrCommencedCargoOp = arrival.CommencedCargoOperation;
                dto.ArrTugsIn = arrival.TugsIn;
                dto.ArrDraftFwd = arrival.ArrivalDraftFwdMtr;
                dto.ArrDraftAft = arrival.ArrivalDraftAftMtr;
                dto.ArrDraftMean = arrival.ArrivalDraftMeanMtr;
                dto.ArrFreshWater = arrival.FreshWater;
                dto.ArrBallastWater = arrival.BallastWater;
                dto.ArrRemarks = arrival.Remarks;
            }

            // Populate actual dates from departure
            if (departure != null)
            {
                dto.ActualETD = departure.ActualETD;
                dto.DepCompleteCargoOp = departure.CompleteCargoOperation;
                dto.DepPilotOnBoard = departure.PilotOnBoard;
                dto.DepUnberthFAOP = departure.UnberthFAOP;
                dto.DepTugsOut = departure.TugsOut;
                dto.DepDraftFwd = departure.DepDraftFwdMtr;
                dto.DepDraftAft = departure.DepDraftAftMtr;
                dto.DepDraftMean = departure.DepDraftMeanMtr;
                dto.DepFreshWater = departure.FreshWater;
                dto.DepBallastWater = departure.BallastWater;
                dto.DepRemarks = departure.Remarks;
            }

            result.Add(dto);
        }
        return result;
    }

    public async Task<Guid> CreateAsync(VoyageCreateDto dto, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var voyage = new Voyage
        {
            Id = Guid.NewGuid(),
            VesselId = dto.VesselId,
            ServiceId = dto.ServiceId,
            OperatorId = dto.OperatorId,
            VoyageType = dto.VoyageType,
            IsDeleted = false,
            CreatedOn = now,
            ModifiedOn = now,
            CreatedBy = dto.CreatedBy,
            ModifiedBy = dto.CreatedBy
        };

        await _voyagesRepo.AddAsync(voyage, ct);

        if (dto.Ports.Any())
        {
            var portEntities = dto.Ports.Select((p, i) => new VoyagePort
            {
                Id = Guid.NewGuid(),
                VoyageId = voyage.Id,
                VoyNo = p.VoyNo,
                Bound = p.Bound,
                PortId = p.PortId,
                TerminalId = p.TerminalId,
                ETA = p.ETA,
                ETB = p.ETB,
                ETD = p.ETD,
                PortStay = p.PortStay,
                SeaDay = p.SeaDay,
                Speed = p.Speed,
                Distance = p.Distance,
                SortOrder = p.SortOrder > 0 ? p.SortOrder : i + 1,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            }).ToList();

            await _portsRepo.AddRangeAsync(portEntities, ct);
        }

        return voyage.Id;
    }

    public async Task UpdateAsync(VoyageUpdateDto dto, CancellationToken ct = default)
    {
        var voyage = await _voyagesRepo.GetByIdAsync(dto.Id, ct)
            ?? throw new Exception("Voyage not found.");

        voyage.VesselId = dto.VesselId;
        voyage.ServiceId = dto.ServiceId;
        voyage.OperatorId = dto.OperatorId;
        voyage.VoyageType = dto.VoyageType;
        voyage.ModifiedOn = DateTime.UtcNow;
        voyage.ModifiedBy = dto.ModifiedBy;

        await _voyagesRepo.UpdateAsync(voyage, ct);

        // Replace all port entries
        await _portsRepo.DeleteByVoyageIdAsync(dto.Id, ct);

        if (dto.Ports.Any())
        {
            var now = DateTime.UtcNow;
            var portEntities = dto.Ports.Select((p, i) => new VoyagePort
            {
                Id = Guid.NewGuid(),
                VoyageId = dto.Id,
                VoyNo = p.VoyNo,
                Bound = p.Bound,
                PortId = p.PortId,
                TerminalId = p.TerminalId,
                ETA = p.ETA,
                ETB = p.ETB,
                ETD = p.ETD,
                PortStay = p.PortStay,
                SeaDay = p.SeaDay,
                Speed = p.Speed,
                Distance = p.Distance,
                SortOrder = p.SortOrder > 0 ? p.SortOrder : i + 1,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.ModifiedBy,
                ModifiedBy = dto.ModifiedBy
            }).ToList();

            await _portsRepo.AddRangeAsync(portEntities, ct);
        }
    }

    public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
    {
        await _voyagesRepo.SoftDeleteAsync(id, modifiedBy, ct);
    }

    // ── Schedule Viewer: one row per voyage-port entry (as entered) ──
    public async Task<List<ScheduleRowDto>> GetScheduleRowsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        Guid? serviceId = null,
        Guid? portId = null,
        Guid? polId = null,
        Guid? podId = null,
        CancellationToken ct = default)
    {
        var voyages = await _voyagesRepo.GetAllAsync(ct);
        var vessels = await _vesselsRepo.GetAllAsync(ct);
        var services = await _servicesRepo.GetAllAsync(ct);
        var allPorts = await _portsMasterRepo.GetAllAsync(ct);
        var terminals = await _terminalsRepo.GetAllAsync(ct);

        var operators = await _operatorsRepo.GetAllAsync(ct);

        var vesselLookup = vessels.ToDictionary(v => v.Id);
        var serviceLookup = services.ToDictionary(s => s.Id);
        var operatorLookup = operators.ToDictionary(o => o.Id);
        var portLookup = allPorts.ToDictionary(p => p.Id);
        var termLookup = terminals.ToDictionary(t => t.Id);

        // Pre-filter by service
        if (serviceId.HasValue)
            voyages = voyages.Where(v => v.ServiceId == serviceId.Value).ToList();

        var rows = new List<ScheduleRowDto>();

        foreach (var voyage in voyages)
        {
            var ports = await _portsRepo.GetByVoyageIdAsync(voyage.Id, ct);
            var sorted = ports.OrderBy(p => p.SortOrder).ToList();

            if (sorted.Count == 0) continue;

            var vsl = vesselLookup.TryGetValue(voyage.VesselId, out var v) ? v : null;
            var svc = voyage.ServiceId.HasValue && serviceLookup.TryGetValue(voyage.ServiceId.Value, out var s) ? s : null;
            var opr = voyage.OperatorId.HasValue && operatorLookup.TryGetValue(voyage.OperatorId.Value, out var o) ? o : null;

            // Check if any port in this voyage matches the port/pol/pod filters
            bool voyageMatchesPortFilter = !portId.HasValue
                || sorted.Any(p => p.PortId == portId.Value);
            bool voyageMatchesPolFilter = !polId.HasValue
                || sorted.Any(p => p.PortId == polId.Value);
            bool voyageMatchesPodFilter = !podId.HasValue
                || sorted.Any(p => p.PortId == podId.Value);

            if (!voyageMatchesPortFilter || !voyageMatchesPolFilter || !voyageMatchesPodFilter)
                continue;

            // Date range: check if any port in this voyage falls in range
            if (fromDate.HasValue || toDate.HasValue)
            {
                bool anyInRange = sorted.Any(p =>
                {
                    var etd = p.ETD ?? p.ETA;
                    if (etd == null) return true; // include ports without dates
                    if (fromDate.HasValue && etd.Value < fromDate.Value) return false;
                    if (toDate.HasValue && etd.Value > toDate.Value.AddDays(1)) return false;
                    return true;
                });
                if (!anyInRange) continue;
            }

            for (int i = 0; i < sorted.Count; i++)
            {
                var vp = sorted[i];
                var port = portLookup.TryGetValue(vp.PortId, out var p2) ? p2 : null;
                var term = vp.TerminalId.HasValue && termLookup.TryGetValue(vp.TerminalId.Value, out var t2) ? t2 : null;

                rows.Add(new ScheduleRowDto
                {
                    VoyageId = voyage.Id,
                    Service = svc?.ServiceCode,
                    VesselCode = vsl?.VesselCode,
                    VesselName = vsl?.VesselName,
                    Operator = opr?.OperatorName,
                    Slot = voyage.VoyageType,
                    Voyage = vp.VoyNo,
                    Bound = vp.Bound,
                    Leg = i + 1,
                    TotalLegs = sorted.Count,
                    Port = port?.PortCode ?? port?.FullName,
                    Terminal = term?.TerminalCode ?? term?.TerminalName,
                    ETA = vp.ETA,
                    ETB = vp.ETB,
                    ETD = vp.ETD,
                    PortStay = vp.PortStay,
                    SeaDay = vp.SeaDay,
                    Speed = vp.Speed,
                    Distance = vp.Distance
                });
            }
        }

        return rows;
    }

    // ── Vessel Arrival ──

    public async Task<VoyagePortArrivalDto?> GetArrivalByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default)
    {
        var arrival = await _arrivalsRepo.GetByVoyagePortIdAsync(voyagePortId, ct);
        if (arrival == null) return null;

        var ports = await _portsMasterRepo.GetAllAsync(ct);
        var portLookup = ports.ToDictionary(p => p.Id, p => p.PortCode ?? p.FullName);

        var dto = new VoyagePortArrivalDto
        {
            Id = arrival.Id,
            VoyagePortId = arrival.VoyagePortId,
            InboundVoyage = arrival.InboundVoyage,
            OutboundVoyage = arrival.OutboundVoyage,
            ActualETA = arrival.ActualETA,
            ActualETB = arrival.ActualETB,
            LastPortId = arrival.LastPortId,
            LastPortCode = arrival.LastPortId.HasValue && portLookup.TryGetValue(arrival.LastPortId.Value, out var lp) ? lp : null,
            NextPortId = arrival.NextPortId,
            NextPortCode = arrival.NextPortId.HasValue && portLookup.TryGetValue(arrival.NextPortId.Value, out var np) ? np : null,
            PilotOnBoard = arrival.PilotOnBoard,
            CommencedCargoOperation = arrival.CommencedCargoOperation,
            TugsIn = arrival.TugsIn,
            ArrivalDraftFwdMtr = arrival.ArrivalDraftFwdMtr,
            ArrivalDraftAftMtr = arrival.ArrivalDraftAftMtr,
            ArrivalDraftMeanMtr = arrival.ArrivalDraftMeanMtr,
            FreshWater = arrival.FreshWater,
            BallastWater = arrival.BallastWater,
            Remarks = arrival.Remarks
        };

        // Load tug usages
        var tugUsages = await _tugUsageRepo.GetByArrivalIdAsync(arrival.Id, ct);
        dto.TugUsages = tugUsages.Select(t => new TugUsageDto
        {
            Id = t.Id,
            TugNumber = t.TugNumber,
            Hours = t.Hours
        }).ToList();

        // Load bunker on arrival readings
        var bunkers = await _bunkerRepo.GetByArrivalIdAsync(arrival.Id, ct);
        dto.Bunkers = bunkers.Select(b => new BunkerOnArrivalDto
        {
            ReadingPoint = b.ReadingPoint,
            VlsfoMts = b.VlsfoMts,
            MgoMts = b.MgoMts,
            HfoMts = b.HfoMts
        }).ToList();

        return dto;
    }

    public async Task<Guid> SaveArrivalAsync(VoyagePortArrivalDto dto, CancellationToken ct = default)
    {
        var existing = await _arrivalsRepo.GetByVoyagePortIdAsync(dto.VoyagePortId, ct);

        if (existing != null)
        {
            existing.InboundVoyage = dto.InboundVoyage;
            existing.OutboundVoyage = dto.OutboundVoyage;
            existing.ActualETA = dto.ActualETA;
            existing.ActualETB = dto.ActualETB;
            existing.LastPortId = dto.LastPortId;
            existing.NextPortId = dto.NextPortId;
            existing.PilotOnBoard = dto.PilotOnBoard;
            existing.CommencedCargoOperation = dto.CommencedCargoOperation;
            existing.TugsIn = dto.TugsIn;
            existing.ArrivalDraftFwdMtr = dto.ArrivalDraftFwdMtr;
            existing.ArrivalDraftAftMtr = dto.ArrivalDraftAftMtr;
            existing.ArrivalDraftMeanMtr = dto.ArrivalDraftMeanMtr;
            existing.FreshWater = dto.FreshWater;
            existing.BallastWater = dto.BallastWater;
            existing.Remarks = dto.Remarks;
            existing.ModifiedBy = dto.ModifiedBy;

            await _arrivalsRepo.UpdateAsync(existing, ct);

            // Save tug usage rows
            await SaveTugUsagesForArrivalAsync(existing.Id, dto.TugUsages, dto.ModifiedBy, ct);

            // Save bunker on arrival readings
            await SaveBunkersForArrivalAsync(existing.Id, dto.Bunkers, dto.ModifiedBy, ct);

            // Update ETD on VoyagePort if provided
            if (dto.EstimatedETD.HasValue)
                await _portsRepo.UpdateETDAsync(dto.VoyagePortId, dto.EstimatedETD.Value, dto.ModifiedBy, ct);

            return existing.Id;
        }
        else
        {
            var arrival = new VoyagePortArrival
            {
                Id = Guid.NewGuid(),
                VoyagePortId = dto.VoyagePortId,
                InboundVoyage = dto.InboundVoyage,
                OutboundVoyage = dto.OutboundVoyage,
                ActualETA = dto.ActualETA,
                ActualETB = dto.ActualETB,
                LastPortId = dto.LastPortId,
                NextPortId = dto.NextPortId,
                PilotOnBoard = dto.PilotOnBoard,
                CommencedCargoOperation = dto.CommencedCargoOperation,
                TugsIn = dto.TugsIn,
                ArrivalDraftFwdMtr = dto.ArrivalDraftFwdMtr,
                ArrivalDraftAftMtr = dto.ArrivalDraftAftMtr,
                ArrivalDraftMeanMtr = dto.ArrivalDraftMeanMtr,
                FreshWater = dto.FreshWater,
                BallastWater = dto.BallastWater,
                Remarks = dto.Remarks,
                CreatedBy = dto.CreatedBy
            };

            await _arrivalsRepo.AddAsync(arrival, ct);

            // Save tug usage rows
            await SaveTugUsagesForArrivalAsync(arrival.Id, dto.TugUsages, dto.CreatedBy, ct);

            // Save bunker on arrival readings
            await SaveBunkersForArrivalAsync(arrival.Id, dto.Bunkers, dto.CreatedBy, ct);

            // Update ETD on VoyagePort if provided
            if (dto.EstimatedETD.HasValue)
                await _portsRepo.UpdateETDAsync(dto.VoyagePortId, dto.EstimatedETD.Value, dto.CreatedBy, ct);

            return arrival.Id;
        }
    }

    private async Task SaveTugUsagesForArrivalAsync(Guid arrivalId, List<TugUsageDto> tugDtos, Guid userId, CancellationToken ct)
    {
        var tugs = tugDtos.Select(t => new TugUsage
        {
            TugNumber = t.TugNumber,
            Hours = t.Hours,
            CreatedBy = userId,
            ModifiedBy = userId
        }).ToList();

        await _tugUsageRepo.ReplaceForArrivalAsync(arrivalId, tugs, ct);
    }

    private async Task SaveTugUsagesForDepartureAsync(Guid departureId, List<TugUsageDto> tugDtos, Guid userId, CancellationToken ct)
    {
        var tugs = tugDtos.Select(t => new TugUsage
        {
            TugNumber = t.TugNumber,
            Hours = t.Hours,
            CreatedBy = userId,
            ModifiedBy = userId
        }).ToList();

        await _tugUsageRepo.ReplaceForDepartureAsync(departureId, tugs, ct);
    }

    private async Task SaveBunkersForArrivalAsync(Guid arrivalId, List<BunkerOnArrivalDto> bunkerDtos, Guid userId, CancellationToken ct)
    {
        var bunkers = bunkerDtos.Select(b => new BunkerOnArrival
        {
            ReadingPoint = b.ReadingPoint,
            VlsfoMts = b.VlsfoMts,
            MgoMts = b.MgoMts,
            HfoMts = b.HfoMts,
            CreatedBy = userId,
            ModifiedBy = userId
        }).ToList();

        await _bunkerRepo.ReplaceForArrivalAsync(arrivalId, bunkers, ct);
    }

    private async Task SaveBunkersForDepartureAsync(Guid departureId, List<BunkerOnDepartureDto> bunkerDtos, Guid userId, CancellationToken ct)
    {
        var bunkers = bunkerDtos.Select(b => new BunkerOnDeparture
        {
            ReadingPoint = b.ReadingPoint,
            VlsfoMts = b.VlsfoMts,
            MgoMts = b.MgoMts,
            HfoMts = b.HfoMts,
            CreatedBy = userId,
            ModifiedBy = userId
        }).ToList();

        await _bunkerDepRepo.ReplaceForDepartureAsync(departureId, bunkers, ct);
    }

    private async Task SaveBunkerSupplyAsync(Guid departureId, List<BunkerSupplyDto> supplyDtos, Guid userId, CancellationToken ct)
    {
        var supplies = supplyDtos.Select(s => new BunkerSupply
        {
            FuelType = s.FuelType,
            Qty = s.Qty,
            RateMts = s.RateMts,
            CreatedBy = userId,
            ModifiedBy = userId
        }).ToList();

        await _bunkerSupplyRepo.ReplaceForDepartureAsync(departureId, supplies, ct);
    }

    // ── Vessel Departure ──

    public async Task<List<BunkerOnArrivalDto>> GetArrivalBunkersByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default)
    {
        var arrival = await _arrivalsRepo.GetByVoyagePortIdAsync(voyagePortId, ct);
        if (arrival == null) return new List<BunkerOnArrivalDto>();

        var arrivalBunkers = await _bunkerRepo.GetByArrivalIdAsync(arrival.Id, ct);
        return arrivalBunkers.Select(b => new BunkerOnArrivalDto
        {
            ReadingPoint = b.ReadingPoint,
            VlsfoMts = b.VlsfoMts,
            MgoMts = b.MgoMts,
            HfoMts = b.HfoMts
        }).ToList();
    }

    public async Task<VoyagePortDepartureDto?> GetDepartureByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default)
    {
        var dep = await _departuresRepo.GetByVoyagePortIdAsync(voyagePortId, ct);
        if (dep == null) return null;

        var ports = await _portsMasterRepo.GetAllAsync(ct);
        var portLookup = ports.ToDictionary(p => p.Id, p => p.PortCode ?? p.FullName);

        var result = new VoyagePortDepartureDto
        {
            Id = dep.Id,
            VoyagePortId = dep.VoyagePortId,
            InboundVoyage = dep.InboundVoyage,
            OutboundVoyage = dep.OutboundVoyage,
            CompleteCargoOperation = dep.CompleteCargoOperation,
            PilotOnBoard = dep.PilotOnBoard,
            UnberthFAOP = dep.UnberthFAOP,
            ActualETD = dep.ActualETD,
            NextPortId = dep.NextPortId,
            NextPortCode = dep.NextPortId.HasValue && portLookup.TryGetValue(dep.NextPortId.Value, out var np) ? np : null,
            ETANextPort = dep.ETANextPort,
            TugsOut = dep.TugsOut,
            DepDraftFwdMtr = dep.DepDraftFwdMtr,
            DepDraftAftMtr = dep.DepDraftAftMtr,
            DepDraftMeanMtr = dep.DepDraftMeanMtr,
            FreshWater = dep.FreshWater,
            BallastWater = dep.BallastWater,
            Remarks = dep.Remarks
        };

        // Load tug usages
        var tugUsages = await _tugUsageRepo.GetByDepartureIdAsync(dep.Id, ct);
        result.TugUsages = tugUsages.Select(t => new TugUsageDto
        {
            Id = t.Id,
            TugNumber = t.TugNumber,
            Hours = t.Hours
        }).ToList();

        // Load bunker supply (received)
        var supplies = await _bunkerSupplyRepo.GetByDepartureIdAsync(dep.Id, ct);
        result.BunkerSupplies = supplies.Select(s => new BunkerSupplyDto
        {
            FuelType = s.FuelType,
            Qty = s.Qty,
            RateMts = s.RateMts
        }).ToList();

        // Load bunker on departure readings
        var depBunkers = await _bunkerDepRepo.GetByDepartureIdAsync(dep.Id, ct);
        result.Bunkers = depBunkers.Select(b => new BunkerOnDepartureDto
        {
            ReadingPoint = b.ReadingPoint,
            VlsfoMts = b.VlsfoMts,
            MgoMts = b.MgoMts,
            HfoMts = b.HfoMts
        }).ToList();

        // Load arrival bunkers for the SAME VoyagePortId (for departure validation baseline)
        var arrival = await _arrivalsRepo.GetByVoyagePortIdAsync(voyagePortId, ct);
        if (arrival != null)
        {
            var arrivalBunkers = await _bunkerRepo.GetByArrivalIdAsync(arrival.Id, ct);
            result.ArrivalBunkers = arrivalBunkers.Select(b => new BunkerOnArrivalDto
            {
                ReadingPoint = b.ReadingPoint,
                VlsfoMts = b.VlsfoMts,
                MgoMts = b.MgoMts,
                HfoMts = b.HfoMts
            }).ToList();
        }

        return result;
    }

    public async Task<Guid> SaveDepartureAsync(VoyagePortDepartureDto dto, CancellationToken ct = default)
    {
        var existing = await _departuresRepo.GetByVoyagePortIdAsync(dto.VoyagePortId, ct);

        if (existing != null)
        {
            existing.InboundVoyage = dto.InboundVoyage;
            existing.OutboundVoyage = dto.OutboundVoyage;
            existing.CompleteCargoOperation = dto.CompleteCargoOperation;
            existing.PilotOnBoard = dto.PilotOnBoard;
            existing.UnberthFAOP = dto.UnberthFAOP;
            existing.ActualETD = dto.ActualETD;
            existing.NextPortId = dto.NextPortId;
            existing.ETANextPort = dto.ETANextPort;
            existing.TugsOut = dto.TugsOut;
            existing.DepDraftFwdMtr = dto.DepDraftFwdMtr;
            existing.DepDraftAftMtr = dto.DepDraftAftMtr;
            existing.DepDraftMeanMtr = dto.DepDraftMeanMtr;
            existing.FreshWater = dto.FreshWater;
            existing.BallastWater = dto.BallastWater;
            existing.Remarks = dto.Remarks;
            existing.ModifiedBy = dto.ModifiedBy;

            await _departuresRepo.UpdateAsync(existing, ct);

            // Save tug usage rows
            await SaveTugUsagesForDepartureAsync(existing.Id, dto.TugUsages, dto.ModifiedBy, ct);

            // Save bunker supply
            await SaveBunkerSupplyAsync(existing.Id, dto.BunkerSupplies, dto.ModifiedBy, ct);

            // Save bunker on departure readings
            await SaveBunkersForDepartureAsync(existing.Id, dto.Bunkers, dto.ModifiedBy, ct);

            if (dto.ActualETD.HasValue)
                await _portsRepo.UpdateETDAsync(dto.VoyagePortId, dto.ActualETD.Value, dto.ModifiedBy, ct);

            return existing.Id;
        }
        else
        {
            var departure = new VoyagePortDeparture
            {
                Id = Guid.NewGuid(),
                VoyagePortId = dto.VoyagePortId,
                InboundVoyage = dto.InboundVoyage,
                OutboundVoyage = dto.OutboundVoyage,
                CompleteCargoOperation = dto.CompleteCargoOperation,
                PilotOnBoard = dto.PilotOnBoard,
                UnberthFAOP = dto.UnberthFAOP,
                ActualETD = dto.ActualETD,
                NextPortId = dto.NextPortId,
                ETANextPort = dto.ETANextPort,
                TugsOut = dto.TugsOut,
                DepDraftFwdMtr = dto.DepDraftFwdMtr,
                DepDraftAftMtr = dto.DepDraftAftMtr,
                DepDraftMeanMtr = dto.DepDraftMeanMtr,
                FreshWater = dto.FreshWater,
                BallastWater = dto.BallastWater,
                Remarks = dto.Remarks,
                CreatedBy = dto.CreatedBy
            };

            await _departuresRepo.AddAsync(departure, ct);

            // Save tug usage rows
            await SaveTugUsagesForDepartureAsync(departure.Id, dto.TugUsages, dto.CreatedBy, ct);

            // Save bunker supply
            await SaveBunkerSupplyAsync(departure.Id, dto.BunkerSupplies, dto.CreatedBy, ct);

            // Save bunker on departure readings
            await SaveBunkersForDepartureAsync(departure.Id, dto.Bunkers, dto.CreatedBy, ct);

            if (dto.ActualETD.HasValue)
                await _portsRepo.UpdateETDAsync(dto.VoyagePortId, dto.ActualETD.Value, dto.CreatedBy, ct);

            return departure.Id;
        }
    }

    // ── TOS (Terminal Operating System) ──

    public async Task<TosDto?> GetTosByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default)
    {
        var tos = await _tosRepo.GetByVoyagePortIdAsync(voyagePortId, ct);
        if (tos == null) return null;

        var dto = new TosDto
        {
            Id = tos.Id,
            VoyagePortId = tos.VoyagePortId,
            DischargeMoves = tos.DischargeMoves,
            LoadMoves = tos.LoadMoves,
            Restows = tos.Restows,
            TotalHatchCover = tos.TotalHatchCover,
            NoOfBin = tos.NoOfBin,
            TotalMoves = tos.TotalMoves
        };

        // Load crane productivity (separate table)
        var cp = await _craneProductivityRepo.GetByTosIdAsync(tos.Id, ct);
        if (cp != null)
        {
            dto.CraneProductivity = new CraneProductivityDto
            {
                Id = cp.Id,
                CranesPerSI = cp.CranesPerSI,
                ActualCranes = cp.ActualCranes,
                TotalOpsTime = cp.TotalOpsTime,
                NonProductiveTime = cp.NonProductiveTime,
                CraneWorkingHrs = cp.CraneWorkingHrs,
                CraneProductivityPerHr = cp.CraneProductivityPerHr,
                MovesPerCrane = cp.MovesPerCrane
            };
        }

        // Load ship productivity (separate table)
        var sp = await _shipProductivityRepo.GetByTosIdAsync(tos.Id, ct);
        if (sp != null)
        {
            dto.ShipProductivity = new ShipProductivityDto
            {
                Id = sp.Id,
                PortStayTime = sp.PortStayTime,
                ProductivityPerHr = sp.ProductivityPerHr,
                MovesPerCrane = sp.MovesPerCrane
            };
        }

        // Load TOS summary (separate table)
        var summary = await _tosSummaryRepo.GetByTosIdAsync(tos.Id, ct);
        if (summary != null)
        {
            dto.Summary = new TosSummaryDto
            {
                Id = summary.Id,
                VesselTurnaroundTime = summary.VesselTurnaroundTime,
                BerthingDelay = summary.BerthingDelay,
                TotalMoves = summary.TotalMoves,
                NonProductiveTime = summary.NonProductiveTime,
                TerminalProductivity = summary.TerminalProductivity,
                ShipProductivity = summary.ShipProductivity
            };
        }

        var stoppages = await _tosStoppageRepo.GetByTosIdAsync(tos.Id, ct);
        dto.Stoppages = stoppages.Select(s => new TosStoppageDto
        {
            Id = s.Id,
            RowNumber = s.RowNumber,
            FromDateTime = s.FromDateTime,
            ToDateTime = s.ToDateTime,
            NonProductiveHours = s.NonProductiveHours,
            Reason = s.Reason
        }).ToList();

        return dto;
    }

    public async Task<Guid> SaveTosAsync(TosDto dto, CancellationToken ct = default)
    {
        var existing = await _tosRepo.GetByVoyagePortIdAsync(dto.VoyagePortId, ct);

        Guid tosId;
        if (existing != null)
        {
            existing.DischargeMoves = dto.DischargeMoves;
            existing.LoadMoves = dto.LoadMoves;
            existing.Restows = dto.Restows;
            existing.TotalHatchCover = dto.TotalHatchCover;
            existing.NoOfBin = dto.NoOfBin;
            existing.TotalMoves = dto.TotalMoves;
            existing.ModifiedBy = dto.ModifiedBy;

            await _tosRepo.UpdateAsync(existing, ct);
            tosId = existing.Id;
        }
        else
        {
            var tos = new Tos
            {
                Id = Guid.NewGuid(),
                VoyagePortId = dto.VoyagePortId,
                DischargeMoves = dto.DischargeMoves,
                LoadMoves = dto.LoadMoves,
                Restows = dto.Restows,
                TotalHatchCover = dto.TotalHatchCover,
                NoOfBin = dto.NoOfBin,
                TotalMoves = dto.TotalMoves,
                CreatedBy = dto.CreatedBy
            };

            await _tosRepo.AddAsync(tos, ct);
            tosId = tos.Id;
        }

        // Save stoppages — skip entirely empty rows
        var stoppages = dto.Stoppages
            .Where(s => s.FromDateTime.HasValue || s.ToDateTime.HasValue || s.NonProductiveHours.HasValue || !string.IsNullOrWhiteSpace(s.Reason))
            .Select((s, idx) => new TosStoppage
            {
                RowNumber = idx + 1,
                FromDateTime = s.FromDateTime,
                ToDateTime = s.ToDateTime,
                NonProductiveHours = s.NonProductiveHours,
                Reason = s.Reason,
                CreatedBy = dto.ModifiedBy != Guid.Empty ? dto.ModifiedBy : dto.CreatedBy,
                ModifiedBy = dto.ModifiedBy != Guid.Empty ? dto.ModifiedBy : dto.CreatedBy
            }).ToList();

        await _tosStoppageRepo.ReplaceForTosAsync(tosId, stoppages, ct);

        // Save Crane Productivity (separate table)
        if (dto.CraneProductivity != null)
        {
            var cp = new CraneProductivity
            {
                TosId = tosId,
                CranesPerSI = dto.CraneProductivity.CranesPerSI,
                ActualCranes = dto.CraneProductivity.ActualCranes,
                TotalOpsTime = dto.CraneProductivity.TotalOpsTime,
                NonProductiveTime = dto.CraneProductivity.NonProductiveTime,
                CraneWorkingHrs = dto.CraneProductivity.CraneWorkingHrs,
                CraneProductivityPerHr = dto.CraneProductivity.CraneProductivityPerHr,
                MovesPerCrane = dto.CraneProductivity.MovesPerCrane,
                CreatedBy = dto.ModifiedBy != Guid.Empty ? dto.ModifiedBy : dto.CreatedBy,
                ModifiedBy = dto.ModifiedBy != Guid.Empty ? dto.ModifiedBy : dto.CreatedBy
            };
            await _craneProductivityRepo.UpsertAsync(cp, ct);
        }

        // Save Ship Productivity (separate table)
        if (dto.ShipProductivity != null)
        {
            var sp = new ShipProductivity
            {
                TosId = tosId,
                PortStayTime = dto.ShipProductivity.PortStayTime,
                ProductivityPerHr = dto.ShipProductivity.ProductivityPerHr,
                MovesPerCrane = dto.ShipProductivity.MovesPerCrane,
                CreatedBy = dto.ModifiedBy != Guid.Empty ? dto.ModifiedBy : dto.CreatedBy,
                ModifiedBy = dto.ModifiedBy != Guid.Empty ? dto.ModifiedBy : dto.CreatedBy
            };
            await _shipProductivityRepo.UpsertAsync(sp, ct);
        }

        // Save TOS Summary (separate table)
        if (dto.Summary != null)
        {
            var summary = new TosSummary
            {
                TosId = tosId,
                VesselTurnaroundTime = dto.Summary.VesselTurnaroundTime,
                BerthingDelay = dto.Summary.BerthingDelay,
                TotalMoves = dto.Summary.TotalMoves,
                NonProductiveTime = dto.Summary.NonProductiveTime,
                TerminalProductivity = dto.Summary.TerminalProductivity,
                ShipProductivity = dto.Summary.ShipProductivity,
                CreatedBy = dto.ModifiedBy != Guid.Empty ? dto.ModifiedBy : dto.CreatedBy,
                ModifiedBy = dto.ModifiedBy != Guid.Empty ? dto.ModifiedBy : dto.CreatedBy
            };
            await _tosSummaryRepo.UpsertAsync(summary, ct);
        }

        return tosId;
    }
}
