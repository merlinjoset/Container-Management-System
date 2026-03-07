using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Routes;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class RoutesRepository : IRoutesRepository
    {
        private readonly AppDbContext _context;

        public RoutesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Route>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<RouteEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.RouteName)
                .Select(x => new Route
                {
                    Id = x.Id,
                    RouteName = x.RouteName,
                    PortOfOriginId = x.PortOfOriginId,
                    FinalDestinationId = x.FinalDestinationId,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<Route?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<RouteEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new Route
                {
                    Id = x.Id,
                    RouteName = x.RouteName,
                    PortOfOriginId = x.PortOfOriginId,
                    FinalDestinationId = x.FinalDestinationId,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(string routeName, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<RouteEntity>()
                .Where(x => x.RouteName == routeName && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Route route, CancellationToken ct = default)
        {
            var entity = new RouteEntity
            {
                Id = route.Id == Guid.Empty ? Guid.NewGuid() : route.Id,
                RouteName = route.RouteName,
                PortOfOriginId = route.PortOfOriginId,
                FinalDestinationId = route.FinalDestinationId,
                IsDeleted = false,
                CreatedOn = route.CreatedOn == default ? DateTime.UtcNow : route.CreatedOn,
                ModifiedOn = route.ModifiedOn == default ? DateTime.UtcNow : route.ModifiedOn,
                CreatedBy = route.CreatedBy,
                ModifiedBy = route.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            route.Id = entity.Id;
        }

        public async Task UpdateAsync(Route route, CancellationToken ct = default)
        {
            var entity = await _context.Set<RouteEntity>()
                .FirstOrDefaultAsync(x => x.Id == route.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Route not found.");

            entity.RouteName = route.RouteName;
            entity.PortOfOriginId = route.PortOfOriginId;
            entity.FinalDestinationId = route.FinalDestinationId;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = route.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<RouteEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Route not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}
