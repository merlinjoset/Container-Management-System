using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Distances;
using ContainerManagement.Domain.Distances;

namespace ContainerManagement.Application.Services
{
    public class DistanceMasterService
    {
        private readonly IDistancesRepository _repository;
        private readonly IPortsRepository _portsRepository;

        public DistanceMasterService(IDistancesRepository repository, IPortsRepository portsRepository)
        {
            _repository = repository;
            _portsRepository = portsRepository;
        }

        public async Task<List<DistanceListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var distances = await _repository.GetAllAsync(ct);
            var ports = await _portsRepository.GetAllAsync(ct);
            var portLookup = ports.ToDictionary(p => p.Id, p => string.IsNullOrWhiteSpace(p.PortCode) ? p.FullName : $"({p.PortCode}) {p.FullName}");

            return distances.Select(x => new DistanceListItemDto
            {
                Id = x.Id,
                FromPortId = x.FromPortId,
                ToPortId = x.ToPortId,
                Distance = x.Distance,
                FromPortName = portLookup.TryGetValue(x.FromPortId, out var from) ? from : null,
                ToPortName = portLookup.TryGetValue(x.ToPortId, out var to) ? to : null
            }).ToList();
        }

        public async Task<Guid> CreateAsync(DistanceCreateDto dto, CancellationToken ct = default)
        {
            if (dto.FromPortId == dto.ToPortId)
                throw new Exception("From Port and To Port cannot be the same.");

            if (await _repository.ExistsAsync(dto.FromPortId, dto.ToPortId, null, ct))
                throw new Exception("Distance for this port pair already exists.");

            var now = DateTime.UtcNow;

            var distance = new DistanceMaster
            {
                Id = Guid.NewGuid(),
                FromPortId = dto.FromPortId,
                ToPortId = dto.ToPortId,
                Distance = dto.Distance,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _repository.AddAsync(distance, ct);
            return distance.Id;
        }

        public async Task UpdateAsync(DistanceUpdateDto dto, CancellationToken ct = default)
        {
            var distance = await _repository.GetByIdAsync(dto.Id, ct);
            if (distance == null)
                throw new Exception("Distance not found.");

            if (dto.FromPortId == dto.ToPortId)
                throw new Exception("From Port and To Port cannot be the same.");

            if (await _repository.ExistsAsync(dto.FromPortId, dto.ToPortId, dto.Id, ct))
                throw new Exception("Distance for this port pair already exists.");

            distance.FromPortId = dto.FromPortId;
            distance.ToPortId = dto.ToPortId;
            distance.Distance = dto.Distance;
            distance.ModifiedOn = DateTime.UtcNow;
            distance.ModifiedBy = dto.ModifiedBy;

            await _repository.UpdateAsync(distance, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _repository.SoftDeleteAsync(id, modifiedBy, ct);
        }
    }
}
