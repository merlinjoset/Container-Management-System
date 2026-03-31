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
        ITugUsageRepository tugUsageRepo)
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
                HasDeparture = departure != null && departure.ActualETD.HasValue
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
                dto.ArrFuelOil = arrival.FuelOil;
                dto.ArrDieselOil = arrival.DieselOil;
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
                dto.DepFuelOil = departure.FuelOil;
                dto.DepDieselOil = departure.DieselOil;
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
            FuelOil = arrival.FuelOil,
            DieselOil = arrival.DieselOil,
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
            existing.FuelOil = dto.FuelOil;
            existing.DieselOil = dto.DieselOil;
            existing.FreshWater = dto.FreshWater;
            existing.BallastWater = dto.BallastWater;
            existing.Remarks = dto.Remarks;
            existing.ModifiedBy = dto.ModifiedBy;

            await _arrivalsRepo.UpdateAsync(existing, ct);

            // Save tug usage rows
            await SaveTugUsagesForArrivalAsync(existing.Id, dto.TugUsages, dto.ModifiedBy, ct);

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
                FuelOil = dto.FuelOil,
                DieselOil = dto.DieselOil,
                FreshWater = dto.FreshWater,
                BallastWater = dto.BallastWater,
                Remarks = dto.Remarks,
                CreatedBy = dto.CreatedBy
            };

            await _arrivalsRepo.AddAsync(arrival, ct);

            // Save tug usage rows
            await SaveTugUsagesForArrivalAsync(arrival.Id, dto.TugUsages, dto.CreatedBy, ct);

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

    // ── Vessel Departure ──

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
            FuelOil = dep.FuelOil,
            DieselOil = dep.DieselOil,
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
            existing.FuelOil = dto.FuelOil;
            existing.DieselOil = dto.DieselOil;
            existing.FreshWater = dto.FreshWater;
            existing.BallastWater = dto.BallastWater;
            existing.Remarks = dto.Remarks;
            existing.ModifiedBy = dto.ModifiedBy;

            await _departuresRepo.UpdateAsync(existing, ct);

            // Save tug usage rows
            await SaveTugUsagesForDepartureAsync(existing.Id, dto.TugUsages, dto.ModifiedBy, ct);

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
                FuelOil = dto.FuelOil,
                DieselOil = dto.DieselOil,
                FreshWater = dto.FreshWater,
                BallastWater = dto.BallastWater,
                Remarks = dto.Remarks,
                CreatedBy = dto.CreatedBy
            };

            await _departuresRepo.AddAsync(departure, ct);

            // Save tug usage rows
            await SaveTugUsagesForDepartureAsync(departure.Id, dto.TugUsages, dto.CreatedBy, ct);

            if (dto.ActualETD.HasValue)
                await _portsRepo.UpdateETDAsync(dto.VoyagePortId, dto.ActualETD.Value, dto.CreatedBy, ct);

            return departure.Id;
        }
    }
}
