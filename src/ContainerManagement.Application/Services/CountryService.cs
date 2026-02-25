using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Countries;
using ContainerManagement.Domain.Countries;

namespace ContainerManagement.Application.Services
{
    public class CountryService
    {
        private readonly ICountriesRepository _repository;

        public CountryService(ICountriesRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<CountryListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var countries = await _repository.GetAllAsync(ct);
            return countries.Select(x => new CountryListItemDto
            {
                Id = x.Id,
                CountryName = x.CountryName,
                CountryCode = x.CountryCode
            }).ToList();
        }

        public async Task<Guid> CreateAsync(CountryCreateDto dto, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(dto.CountryCode) && await _repository.ExistsAsync(dto.CountryCode!, null, ct))
                throw new Exception("Country code already exists.");

            var now = DateTime.UtcNow;

            var country = new Country
            {
                Id = Guid.NewGuid(),
                CountryName = dto.CountryName,
                CountryCode = dto.CountryCode,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _repository.AddAsync(country, ct);
            return country.Id;
        }

        public async Task UpdateAsync(CountryUpdateDto dto, CancellationToken ct = default)
        {
            var country = await _repository.GetByIdAsync(dto.Id, ct);
            if (country == null)
                throw new Exception("Country not found.");

            if (!string.IsNullOrWhiteSpace(dto.CountryCode) && await _repository.ExistsAsync(dto.CountryCode!, dto.Id, ct))
                throw new Exception("Country code already exists.");

            country.CountryName = dto.CountryName;
            country.CountryCode = dto.CountryCode;
            country.ModifiedOn = DateTime.UtcNow;
            country.ModifiedBy = dto.ModifiedBy;

            await _repository.UpdateAsync(country, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _repository.SoftDeleteAsync(id, modifiedBy, ct);
        }

        public async Task<(int added, int updated, int skipped)> ImportAsync(IEnumerable<(string? Name, string? Code)> rows, Guid userId, CancellationToken ct = default)
        {
            var existing = await _repository.GetAllAsync(ct);
            var byCode = existing
                .Where(c => !string.IsNullOrWhiteSpace(c.CountryCode))
                .ToDictionary(c => c.CountryCode!, c => c, StringComparer.OrdinalIgnoreCase);

            int added = 0, updated = 0, skipped = 0;
            foreach (var (Name, Code) in rows)
            {
                var name = (Name ?? string.Empty).Trim();
                var code = (Code ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(code)) { skipped++; continue; }

                if (!string.IsNullOrWhiteSpace(code) && byCode.TryGetValue(code, out var country))
                {
                    country.CountryName = string.IsNullOrWhiteSpace(name) ? country.CountryName : name;
                    country.ModifiedOn = DateTime.UtcNow;
                    country.ModifiedBy = userId;
                    await _repository.UpdateAsync(country, ct);
                    updated++;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    var newCountry = new Country
                    {
                        Id = Guid.NewGuid(),
                        CountryName = name,
                        CountryCode = string.IsNullOrWhiteSpace(code) ? null : code,
                        IsDeleted = false,
                        CreatedOn = now,
                        ModifiedOn = now,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    };
                    await _repository.AddAsync(newCountry, ct);
                    if (!string.IsNullOrWhiteSpace(newCountry.CountryCode))
                        byCode[newCountry.CountryCode] = newCountry;
                    added++;
                }
            }

            return (added, updated, skipped);
        }
    }
}
