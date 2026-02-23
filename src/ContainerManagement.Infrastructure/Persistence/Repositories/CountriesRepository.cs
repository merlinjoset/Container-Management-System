using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Countries;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class CountriesRepository : ICountriesRepository
    {
        private readonly AppDbContext _context;

        public CountriesRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Country>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<CountryEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.CountryName)
                .Select(x => new Country
                {
                    Id = x.Id,
                    CountryName = x.CountryName,
                    CountryCode = x.CountryCode,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<Country?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<CountryEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new Country
                {
                    Id = x.Id,
                    CountryName = x.CountryName,
                    CountryCode = x.CountryCode,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(string countryCode, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<CountryEntity>()
                .Where(x => x.CountryCode == countryCode && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Country country, CancellationToken ct = default)
        {
            var entity = new CountryEntity
            {
                Id = country.Id == Guid.Empty ? Guid.NewGuid() : country.Id,
                CountryName = country.CountryName,
                CountryCode = country.CountryCode,
                IsDeleted = false,
                CreatedOn = country.CreatedOn == default ? DateTime.UtcNow : country.CreatedOn,
                ModifiedOn = country.ModifiedOn == default ? DateTime.UtcNow : country.ModifiedOn,
                CreatedBy = country.CreatedBy,
                ModifiedBy = country.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            country.Id = entity.Id;
        }

        public async Task UpdateAsync(Country country, CancellationToken ct = default)
        {
            var entity = await _context.Set<CountryEntity>()
                .FirstOrDefaultAsync(x => x.Id == country.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Country not found.");

            entity.CountryName = country.CountryName;
            entity.CountryCode = country.CountryCode;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = country.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<CountryEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Country not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}

