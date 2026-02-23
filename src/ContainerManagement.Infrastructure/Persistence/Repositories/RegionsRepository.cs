using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Regions;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class RegionsRepository : IRegionsRepository
    {
        private readonly AppDbContext _context;

        public RegionsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Region>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<RegionEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.RegionName)
                .Select(x => new Region
                {
                    Id = x.Id,
                    RegionName = x.RegionName,
                    RegionCode = x.RegionCode,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<Region?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<RegionEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new Region
                {
                    Id = x.Id,
                    RegionName = x.RegionName,
                    RegionCode = x.RegionCode,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(string regionCode, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<RegionEntity>()
                .Where(x => x.RegionCode == regionCode && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Region region, CancellationToken ct = default)
        {
            var entity = new RegionEntity
            {
                Id = region.Id == Guid.Empty ? Guid.NewGuid() : region.Id,
                RegionName = region.RegionName,
                RegionCode = region.RegionCode,
                IsDeleted = false,
                CreatedOn = region.CreatedOn == default ? DateTime.UtcNow : region.CreatedOn,
                ModifiedOn = region.ModifiedOn == default ? DateTime.UtcNow : region.ModifiedOn,
                CreatedBy = region.CreatedBy,
                ModifiedBy = region.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            region.Id = entity.Id;
        }

        public async Task UpdateAsync(Region region, CancellationToken ct = default)
        {
            var entity = await _context.Set<RegionEntity>()
                .FirstOrDefaultAsync(x => x.Id == region.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Region not found.");

            entity.RegionName = region.RegionName;
            entity.RegionCode = region.RegionCode;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = region.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<RegionEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Region not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}

