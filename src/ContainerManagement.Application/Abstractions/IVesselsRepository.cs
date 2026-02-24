using ContainerManagement.Domain.Vessels;

namespace ContainerManagement.Application.Abstractions
{
    public interface IVesselsRepository
    {
        Task<List<Vessel>> GetAllAsync(CancellationToken ct = default);
        Task<Vessel?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string vesselCode, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(Vessel vessel, CancellationToken ct = default);
        Task UpdateAsync(Vessel vessel, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}

