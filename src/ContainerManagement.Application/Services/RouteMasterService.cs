using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Routes;
using ContainerManagement.Domain.Routes;

namespace ContainerManagement.Application.Services
{
    public class RouteMasterService
    {
        private readonly IRoutesRepository _repository;
        private readonly IPortsRepository _portsRepository;

        public RouteMasterService(IRoutesRepository repository, IPortsRepository portsRepository)
        {
            _repository = repository;
            _portsRepository = portsRepository;
        }

        public async Task<List<RouteListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var routes = await _repository.GetAllAsync(ct);
            var ports = await _portsRepository.GetAllAsync(ct);
            var portLookup = ports.ToDictionary(p => p.Id, p => string.IsNullOrWhiteSpace(p.PortCode) ? p.FullName : $"({p.PortCode}) {p.FullName}");

            return routes.Select(x => new RouteListItemDto
            {
                Id = x.Id,
                RouteName = x.RouteName,
                PortOfOriginId = x.PortOfOriginId,
                FinalDestinationId = x.FinalDestinationId,
                PortOfOriginName = portLookup.TryGetValue(x.PortOfOriginId, out var origin) ? origin : null,
                FinalDestinationName = portLookup.TryGetValue(x.FinalDestinationId, out var dest) ? dest : null
            }).ToList();
        }

        public async Task<Guid> CreateAsync(RouteCreateDto dto, CancellationToken ct = default)
        {
            if (dto.PortOfOriginId == dto.FinalDestinationId)
                throw new Exception("Port of Origin and Final Destination cannot be the same.");

            if (!string.IsNullOrWhiteSpace(dto.RouteName) && await _repository.ExistsAsync(dto.RouteName!, null, ct))
                throw new Exception("Route name already exists.");

            var now = DateTime.UtcNow;

            var route = new Route
            {
                Id = Guid.NewGuid(),
                RouteName = dto.RouteName,
                PortOfOriginId = dto.PortOfOriginId,
                FinalDestinationId = dto.FinalDestinationId,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _repository.AddAsync(route, ct);
            return route.Id;
        }

        public async Task UpdateAsync(RouteUpdateDto dto, CancellationToken ct = default)
        {
            var route = await _repository.GetByIdAsync(dto.Id, ct);
            if (route == null)
                throw new Exception("Route not found.");

            if (dto.PortOfOriginId == dto.FinalDestinationId)
                throw new Exception("Port of Origin and Final Destination cannot be the same.");

            if (!string.IsNullOrWhiteSpace(dto.RouteName) && await _repository.ExistsAsync(dto.RouteName!, dto.Id, ct))
                throw new Exception("Route name already exists.");

            route.RouteName = dto.RouteName;
            route.PortOfOriginId = dto.PortOfOriginId;
            route.FinalDestinationId = dto.FinalDestinationId;
            route.ModifiedOn = DateTime.UtcNow;
            route.ModifiedBy = dto.ModifiedBy;

            await _repository.UpdateAsync(route, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _repository.SoftDeleteAsync(id, modifiedBy, ct);
        }

        public async Task<(int added, int updated, int skipped)> ImportAsync(IEnumerable<(string? RouteName, string? OriginCode, string? DestCode)> rows, Guid userId, CancellationToken ct = default)
        {
            var existing = await _repository.GetAllAsync(ct);
            var byName = existing
                .Where(r => !string.IsNullOrWhiteSpace(r.RouteName))
                .ToDictionary(r => r.RouteName!, r => r, StringComparer.OrdinalIgnoreCase);

            var ports = await _portsRepository.GetAllAsync(ct);
            var portByCode = ports
                .Where(p => !string.IsNullOrWhiteSpace(p.PortCode))
                .ToDictionary(p => p.PortCode!, p => p.Id, StringComparer.OrdinalIgnoreCase);

            int added = 0, updated = 0, skipped = 0;
            foreach (var (RouteName, OriginCode, DestCode) in rows)
            {
                var name = (RouteName ?? string.Empty).Trim();
                var originCode = (OriginCode ?? string.Empty).Trim();
                var destCode = (DestCode ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }

                if (!portByCode.TryGetValue(originCode, out var originId) || !portByCode.TryGetValue(destCode, out var destId))
                { skipped++; continue; }

                if (byName.TryGetValue(name, out var rt))
                {
                    rt.PortOfOriginId = originId;
                    rt.FinalDestinationId = destId;
                    rt.ModifiedOn = DateTime.UtcNow;
                    rt.ModifiedBy = userId;
                    await _repository.UpdateAsync(rt, ct);
                    updated++;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    var rtNew = new Route
                    {
                        Id = Guid.NewGuid(),
                        RouteName = name,
                        PortOfOriginId = originId,
                        FinalDestinationId = destId,
                        IsDeleted = false,
                        CreatedOn = now,
                        ModifiedOn = now,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    };
                    await _repository.AddAsync(rtNew, ct);
                    byName[name] = rtNew;
                    added++;
                }
            }

            return (added, updated, skipped);
        }
    }
}
