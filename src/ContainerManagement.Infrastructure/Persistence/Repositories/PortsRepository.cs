using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Ports;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class PortsRepository : IPortsRepository
    {
        private readonly AppDbContext _context;

        public PortsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Port>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<PortsEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.PortCode)
                .Select(x => new Port
                {
                    Id = x.Id,
                    PortCode = x.PortCode,
                    FullName = x.FullName,
                    Country = x.Country,
                    Region = x.Region,
                    RegionCode = x.RegionCode,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<Port?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<PortsEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new Port
                {
                    Id = x.Id,
                    PortCode = x.PortCode,
                    FullName = x.FullName,
                    Country = x.Country,
                    Region = x.Region,
                    RegionCode = x.RegionCode,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(string portCode, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<PortsEntity>()
                .Where(x => x.PortCode == portCode && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Port port, CancellationToken ct = default)
        {
            var entity = new PortsEntity
            {
                Id = port.Id == Guid.Empty ? Guid.NewGuid() : port.Id,
                PortCode = port.PortCode,
                FullName = port.FullName,
                Country = port.Country,
                Region = port.Region,
                RegionCode = port.RegionCode,
                IsDeleted = false,
                CreatedOn = port.CreatedOn == default ? DateTime.UtcNow : port.CreatedOn,
                ModifiedOn = port.ModifiedOn == default ? DateTime.UtcNow : port.ModifiedOn,
                CreatedBy = port.CreatedBy,
                ModifiedBy = port.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            // reflect generated Id back if needed
            port.Id = entity.Id;
        }

        public async Task UpdateAsync(Port port, CancellationToken ct = default)
        {
            var entity = await _context.Set<PortsEntity>()
                .FirstOrDefaultAsync(x => x.Id == port.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Port not found.");

            entity.PortCode = port.PortCode;
            entity.FullName = port.FullName;
            entity.Country = port.Country;
            entity.Region = port.Region;
            entity.RegionCode = port.RegionCode;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = port.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<PortsEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Port not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}
