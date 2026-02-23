using ContainerManagement.Domain.Regions;

namespace ContainerManagement.Application.Abstractions
{
    public interface IRegionsRepository
    {
        Task<List<Region>> GetAllAsync(CancellationToken ct = default);
        Task<Region?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string regionCode, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(Region region, CancellationToken ct = default);
        Task UpdateAsync(Region region, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}

