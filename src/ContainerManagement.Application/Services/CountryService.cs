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
    }
}

