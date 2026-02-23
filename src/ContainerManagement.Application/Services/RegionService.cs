using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Regions;
using ContainerManagement.Domain.Regions;

namespace ContainerManagement.Application.Services
{
    public class RegionService
    {
        private readonly IRegionsRepository _repository;

        public RegionService(IRegionsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<RegionListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var regions = await _repository.GetAllAsync(ct);
            return regions.Select(x => new RegionListItemDto
            {
                Id = x.Id,
                RegionName = x.RegionName,
                RegionCode = x.RegionCode
            }).ToList();
        }

        public async Task<Guid> CreateAsync(RegionCreateDto dto, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(dto.RegionCode) && await _repository.ExistsAsync(dto.RegionCode!, null, ct))
                throw new Exception("Region code already exists.");

            var now = DateTime.UtcNow;

            var region = new Region
            {
                Id = Guid.NewGuid(),
                RegionName = dto.RegionName,
                RegionCode = dto.RegionCode,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _repository.AddAsync(region, ct);
            return region.Id;
        }

        public async Task UpdateAsync(RegionUpdateDto dto, CancellationToken ct = default)
        {
            var region = await _repository.GetByIdAsync(dto.Id, ct);
            if (region == null)
                throw new Exception("Region not found.");

            if (!string.IsNullOrWhiteSpace(dto.RegionCode) && await _repository.ExistsAsync(dto.RegionCode!, dto.Id, ct))
                throw new Exception("Region code already exists.");

            region.RegionName = dto.RegionName;
            region.RegionCode = dto.RegionCode;
            region.ModifiedOn = DateTime.UtcNow;
            region.ModifiedBy = dto.ModifiedBy;

            await _repository.UpdateAsync(region, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _repository.SoftDeleteAsync(id, modifiedBy, ct);
        }
    }
}

