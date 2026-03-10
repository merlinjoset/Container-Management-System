using ContainerManagement.Domain.Distances;

namespace ContainerManagement.Application.Abstractions
{
    public interface IDistancesRepository
    {
        Task<List<DistanceMaster>> GetAllAsync(CancellationToken ct = default);
        Task<DistanceMaster?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(Guid fromPortId, Guid toPortId, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(DistanceMaster distance, CancellationToken ct = default);
        Task UpdateAsync(DistanceMaster distance, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}
