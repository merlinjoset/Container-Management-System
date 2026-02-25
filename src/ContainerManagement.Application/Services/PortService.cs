using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Ports;
using ContainerManagement.Domain.Ports;

namespace ContainerManagement.Application.Services
{
    public class PortService
    {
        private readonly IPortsRepository _repository;
        private readonly ICountriesRepository _countries;
        private readonly IRegionsRepository _regions;

        public PortService(IPortsRepository repository, ICountriesRepository countries, IRegionsRepository regions)
        {
            _repository = repository;
            _countries = countries;
            _regions = regions;
        }

        public async Task<List<PortListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var ports = await _repository.GetAllAsync(ct);
            var countryList = await _countries.GetAllAsync(ct);
            var regionList = await _regions.GetAllAsync(ct);
            var cById = countryList.ToDictionary(c => c.Id, c => c);
            var rById = regionList.ToDictionary(r => r.Id, r => r);

            return ports.Select(x => new PortListItemDto
            {
                Id = x.Id,
                PortCode = x.PortCode,
                FullName = x.FullName,
                CountryId = x.CountryId,
                RegionId = x.RegionId,
                CountryName = cById.TryGetValue(x.CountryId, out var c) ? c.CountryName : null,
                RegionName = rById.TryGetValue(x.RegionId, out var r) ? r.RegionName : null
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
                CountryId = dto.CountryId,
                RegionId = dto.RegionId,
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
            port.CountryId = dto.CountryId;
            port.RegionId = dto.RegionId;
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
