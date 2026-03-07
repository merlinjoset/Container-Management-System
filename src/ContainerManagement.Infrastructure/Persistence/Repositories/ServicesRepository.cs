using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Services;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class ServicesRepository : IServicesRepository
    {
        private readonly AppDbContext _context;

        public ServicesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Service>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<ServiceEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.ServiceName)
                .Select(x => new Service
                {
                    Id = x.Id,
                    ServiceCode = x.ServiceCode,
                    ServiceName = x.ServiceName,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<ServiceEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new Service
                {
                    Id = x.Id,
                    ServiceCode = x.ServiceCode,
                    ServiceName = x.ServiceName,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(string serviceCode, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<ServiceEntity>()
                .Where(x => x.ServiceCode == serviceCode && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Service service, CancellationToken ct = default)
        {
            var entity = new ServiceEntity
            {
                Id = service.Id == Guid.Empty ? Guid.NewGuid() : service.Id,
                ServiceCode = service.ServiceCode,
                ServiceName = service.ServiceName,
                IsDeleted = false,
                CreatedOn = service.CreatedOn == default ? DateTime.UtcNow : service.CreatedOn,
                ModifiedOn = service.ModifiedOn == default ? DateTime.UtcNow : service.ModifiedOn,
                CreatedBy = service.CreatedBy,
                ModifiedBy = service.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            service.Id = entity.Id;
        }

        public async Task UpdateAsync(Service service, CancellationToken ct = default)
        {
            var entity = await _context.Set<ServiceEntity>()
                .FirstOrDefaultAsync(x => x.Id == service.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Service not found.");

            entity.ServiceCode = service.ServiceCode;
            entity.ServiceName = service.ServiceName;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = service.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<ServiceEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Service not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}
