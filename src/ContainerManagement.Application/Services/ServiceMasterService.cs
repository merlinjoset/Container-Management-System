using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Services;
using ContainerManagement.Domain.Services;

namespace ContainerManagement.Application.Services
{
    public class ServiceMasterService
    {
        private readonly IServicesRepository _repository;

        public ServiceMasterService(IServicesRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<ServiceListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var services = await _repository.GetAllAsync(ct);
            return services.Select(x => new ServiceListItemDto
            {
                Id = x.Id,
                ServiceCode = x.ServiceCode,
                ServiceName = x.ServiceName
            }).ToList();
        }

        public async Task<Guid> CreateAsync(ServiceCreateDto dto, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(dto.ServiceCode) && await _repository.ExistsAsync(dto.ServiceCode!, null, ct))
                throw new Exception("Service code already exists.");

            var now = DateTime.UtcNow;

            var service = new Service
            {
                Id = Guid.NewGuid(),
                ServiceCode = dto.ServiceCode,
                ServiceName = dto.ServiceName,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _repository.AddAsync(service, ct);
            return service.Id;
        }

        public async Task UpdateAsync(ServiceUpdateDto dto, CancellationToken ct = default)
        {
            var service = await _repository.GetByIdAsync(dto.Id, ct);
            if (service == null)
                throw new Exception("Service not found.");

            if (!string.IsNullOrWhiteSpace(dto.ServiceCode) && await _repository.ExistsAsync(dto.ServiceCode!, dto.Id, ct))
                throw new Exception("Service code already exists.");

            service.ServiceCode = dto.ServiceCode;
            service.ServiceName = dto.ServiceName;
            service.ModifiedOn = DateTime.UtcNow;
            service.ModifiedBy = dto.ModifiedBy;

            await _repository.UpdateAsync(service, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _repository.SoftDeleteAsync(id, modifiedBy, ct);
        }

        public async Task<(int added, int updated, int skipped)> ImportAsync(IEnumerable<(string? Name, string? Code)> rows, Guid userId, CancellationToken ct = default)
        {
            var existing = await _repository.GetAllAsync(ct);
            var byCode = existing
                .Where(s => !string.IsNullOrWhiteSpace(s.ServiceCode))
                .ToDictionary(s => s.ServiceCode!, s => s, StringComparer.OrdinalIgnoreCase);

            int added = 0, updated = 0, skipped = 0;
            foreach (var (Name, Code) in rows)
            {
                var name = (Name ?? string.Empty).Trim();
                var code = (Code ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(code)) { skipped++; continue; }

                if (!string.IsNullOrWhiteSpace(code) && byCode.TryGetValue(code, out var svc))
                {
                    svc.ServiceName = string.IsNullOrWhiteSpace(name) ? svc.ServiceName : name;
                    svc.ModifiedOn = DateTime.UtcNow;
                    svc.ModifiedBy = userId;
                    await _repository.UpdateAsync(svc, ct);
                    updated++;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    var svcNew = new Service
                    {
                        Id = Guid.NewGuid(),
                        ServiceName = name,
                        ServiceCode = string.IsNullOrWhiteSpace(code) ? null : code,
                        IsDeleted = false,
                        CreatedOn = now,
                        ModifiedOn = now,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    };
                    await _repository.AddAsync(svcNew, ct);
                    if (!string.IsNullOrWhiteSpace(svcNew.ServiceCode))
                        byCode[svcNew.ServiceCode] = svcNew;
                    added++;
                }
            }

            return (added, updated, skipped);
        }
    }
}
