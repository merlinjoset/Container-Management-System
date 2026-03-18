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

    public VoyageService(
        IVoyagesRepository voyagesRepo,
        IVoyagePortsRepository portsRepo,
        IVesselsRepository vesselsRepo,
        IServicesRepository servicesRepo,
        IOperatorsRepository operatorsRepo,
        IPortsRepository portsMasterRepo,
        ITerminalsRepository terminalsRepo)
    {
        _voyagesRepo = voyagesRepo;
        _portsRepo = portsRepo;
        _vesselsRepo = vesselsRepo;
        _servicesRepo = servicesRepo;
        _operatorsRepo = operatorsRepo;
        _portsMasterRepo = portsMasterRepo;
        _terminalsRepo = terminalsRepo;
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

        return ports.OrderBy(p => p.SortOrder).Select(p => new VoyagePortListItemDto
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
            SortOrder = p.SortOrder
        }).ToList();
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
}
