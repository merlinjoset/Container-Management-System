using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Ports;
using ContainerManagement.Domain.Ports;

namespace ContainerManagement.Application.Services
{
    public class PortService
    {
        private readonly IPortsRepository _repository;

        public PortService(IPortsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PortListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var ports = await _repository.GetAllAsync(ct);

            return ports.Select(x => new PortListItemDto
            {
                Id = x.Id,
                PortCode = x.PortCode,
                FullName = x.FullName,
                Country = x.Country,
                Region = x.Region,
                RegionCode = x.RegionCode
            }).ToList();
        }

        public async Task<Guid> CreateAsync(PortCreateDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.PortCode) || dto.PortCode.Length != 5)
                throw new Exception("Port Code must be exactly 5 characters.");
            if (await _repository.ExistsAsync(dto.PortCode, null, ct))
                throw new Exception("PortCode already exists.");

            var now = DateTime.UtcNow;

            var port = new Port
            {
                Id = Guid.NewGuid(),
                PortCode = dto.PortCode,
                FullName = dto.FullName,
                Country = dto.Country,
                Region = dto.Region,
                RegionCode = dto.RegionCode,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _repository.AddAsync(port, ct);

            return port.Id;
        }

        public async Task UpdateAsync(PortUpdateDto dto, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(dto.PortCode) || dto.PortCode.Length != 5)
                throw new Exception("Port Code must be exactly 5 characters.");
            var port = await _repository.GetByIdAsync(dto.Id, ct);
            if (port == null)
                throw new Exception("Port not found.");

            if (await _repository.ExistsAsync(dto.PortCode, dto.Id, ct))
                throw new Exception("PortCode already exists.");

            port.PortCode = dto.PortCode;
            port.FullName = dto.FullName;
            port.Country = dto.Country;
            port.Region = dto.Region;
            port.RegionCode = dto.RegionCode;
            port.ModifiedOn = DateTime.UtcNow;
            port.ModifiedBy = dto.ModifiedBy;

            await _repository.UpdateAsync(port, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _repository.SoftDeleteAsync(id, modifiedBy, ct);
        }
    }
}
