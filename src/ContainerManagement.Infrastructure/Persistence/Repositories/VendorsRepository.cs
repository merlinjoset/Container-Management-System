using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Vendors;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class VendorsRepository : IVendorsRepository
    {
        private readonly AppDbContext _context;

        public VendorsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vendor>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<VendorEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.VendorName)
                .Select(x => new Vendor
                {
                    Id = x.Id,
                    VendorName = x.VendorName,
                    VendorCode = x.VendorCode,
                    CountryId = x.CountryId,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<Vendor?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<VendorEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new Vendor
                {
                    Id = x.Id,
                    VendorName = x.VendorName,
                    VendorCode = x.VendorCode,
                    CountryId = x.CountryId,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(string vendorCode, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<VendorEntity>()
                .Where(x => x.VendorCode == vendorCode && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Vendor vendor, CancellationToken ct = default)
        {
            var entity = new VendorEntity
            {
                Id = vendor.Id == Guid.Empty ? Guid.NewGuid() : vendor.Id,
                VendorName = vendor.VendorName,
                VendorCode = vendor.VendorCode,
                CountryId = vendor.CountryId,
                IsDeleted = false,
                CreatedOn = vendor.CreatedOn == default ? DateTime.UtcNow : vendor.CreatedOn,
                ModifiedOn = vendor.ModifiedOn == default ? DateTime.UtcNow : vendor.ModifiedOn,
                CreatedBy = vendor.CreatedBy,
                ModifiedBy = vendor.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            vendor.Id = entity.Id;
        }

        public async Task UpdateAsync(Vendor vendor, CancellationToken ct = default)
        {
            var entity = await _context.Set<VendorEntity>()
                .FirstOrDefaultAsync(x => x.Id == vendor.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Vendor not found.");

            entity.VendorName = vendor.VendorName;
            entity.VendorCode = vendor.VendorCode;
            entity.CountryId = vendor.CountryId;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = vendor.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<VendorEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Vendor not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}

