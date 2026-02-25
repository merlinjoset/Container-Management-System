using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Operators;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class OperatorsRepository : IOperatorsRepository
    {
        private readonly AppDbContext _context;

        public OperatorsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Operator>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<OperatorEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.OperatorName)
                .Select(x => new Operator
                {
                    Id = x.Id,
                    OperatorName = x.OperatorName,
                    VendorId = x.VendorId,
                    CountryId = x.CountryId,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<Operator?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<OperatorEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new Operator
                {
                    Id = x.Id,
                    OperatorName = x.OperatorName,
                    VendorId = x.VendorId,
                    CountryId = x.CountryId,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task AddAsync(Operator op, CancellationToken ct = default)
        {
            var entity = new OperatorEntity
            {
                Id = op.Id == Guid.Empty ? Guid.NewGuid() : op.Id,
                OperatorName = op.OperatorName,
                VendorId = op.VendorId,
                CountryId = op.CountryId,
                IsDeleted = false,
                CreatedOn = op.CreatedOn == default ? DateTime.UtcNow : op.CreatedOn,
                ModifiedOn = op.ModifiedOn == default ? DateTime.UtcNow : op.ModifiedOn,
                CreatedBy = op.CreatedBy,
                ModifiedBy = op.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            op.Id = entity.Id;
        }

        public async Task UpdateAsync(Operator op, CancellationToken ct = default)
        {
            var entity = await _context.Set<OperatorEntity>()
                .FirstOrDefaultAsync(x => x.Id == op.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Operator not found.");

            entity.OperatorName = op.OperatorName;
            entity.VendorId = op.VendorId;
            entity.CountryId = op.CountryId;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = op.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<OperatorEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Operator not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}
