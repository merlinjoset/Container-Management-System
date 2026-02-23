using ContainerManagement.Domain.Countries;

namespace ContainerManagement.Application.Abstractions
{
    public interface ICountriesRepository
    {
        Task<List<Country>> GetAllAsync(CancellationToken ct = default);
        Task<Country?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string countryCode, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(Country country, CancellationToken ct = default);
        Task UpdateAsync(Country country, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}

