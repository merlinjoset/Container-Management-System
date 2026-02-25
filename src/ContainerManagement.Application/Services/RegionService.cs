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

        public async Task<(int added, int updated, int skipped)> ImportAsync(IEnumerable<(string? Name, string? Code)> rows, Guid userId, CancellationToken ct = default)
        {
            var existing = await _repository.GetAllAsync(ct);
            var byCode = existing
                .Where(r => !string.IsNullOrWhiteSpace(r.RegionCode))
                .ToDictionary(r => r.RegionCode!, r => r, StringComparer.OrdinalIgnoreCase);

            int added = 0, updated = 0, skipped = 0;
            foreach (var (Name, Code) in rows)
            {
                var name = (Name ?? string.Empty).Trim();
                var code = (Code ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(code)) { skipped++; continue; }

                if (!string.IsNullOrWhiteSpace(code) && byCode.TryGetValue(code, out var reg))
                {
                    reg.RegionName = string.IsNullOrWhiteSpace(name) ? reg.RegionName : name;
                    reg.ModifiedOn = DateTime.UtcNow;
                    reg.ModifiedBy = userId;
                    await _repository.UpdateAsync(reg, ct);
                    updated++;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    var regNew = new Domain.Regions.Region
                    {
                        Id = Guid.NewGuid(),
                        RegionName = name,
                        RegionCode = string.IsNullOrWhiteSpace(code) ? null : code,
                        IsDeleted = false,
                        CreatedOn = now,
                        ModifiedOn = now,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    };
                    await _repository.AddAsync(regNew, ct);
                    if (!string.IsNullOrWhiteSpace(regNew.RegionCode))
                        byCode[regNew.RegionCode] = regNew;
                    added++;
                }
            }

            return (added, updated, skipped);
        }
    }
}
