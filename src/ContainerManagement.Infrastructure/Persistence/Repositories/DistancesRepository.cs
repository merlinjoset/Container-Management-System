using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Distances;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class DistancesRepository : IDistancesRepository
    {
        private readonly AppDbContext _context;

        public DistancesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<DistanceMaster>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<DistanceEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.FromPortId)
                .Select(x => new DistanceMaster
                {
                    Id = x.Id,
                    FromPortId = x.FromPortId,
                    ToPortId = x.ToPortId,
                    Distance = x.Distance,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<DistanceMaster?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<DistanceEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new DistanceMaster
                {
                    Id = x.Id,
                    FromPortId = x.FromPortId,
                    ToPortId = x.ToPortId,
                    Distance = x.Distance,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(Guid fromPortId, Guid toPortId, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<DistanceEntity>()
                .Where(x => x.FromPortId == fromPortId && x.ToPortId == toPortId && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(DistanceMaster distance, CancellationToken ct = default)
        {
            var entity = new DistanceEntity
            {
                Id = distance.Id == Guid.Empty ? Guid.NewGuid() : distance.Id,
                FromPortId = distance.FromPortId,
                ToPortId = distance.ToPortId,
                Distance = distance.Distance,
                IsDeleted = false,
                CreatedOn = distance.CreatedOn == default ? DateTime.UtcNow : distance.CreatedOn,
                ModifiedOn = distance.ModifiedOn == default ? DateTime.UtcNow : distance.ModifiedOn,
                CreatedBy = distance.CreatedBy,
                ModifiedBy = distance.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            distance.Id = entity.Id;
        }

        public async Task UpdateAsync(DistanceMaster distance, CancellationToken ct = default)
        {
            var entity = await _context.Set<DistanceEntity>()
                .FirstOrDefaultAsync(x => x.Id == distance.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Distance not found.");

            entity.FromPortId = distance.FromPortId;
            entity.ToPortId = distance.ToPortId;
            entity.Distance = distance.Distance;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = distance.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<DistanceEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Distance not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}
