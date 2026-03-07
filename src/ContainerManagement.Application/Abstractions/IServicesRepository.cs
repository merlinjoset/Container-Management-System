using ContainerManagement.Domain.Services;

namespace ContainerManagement.Application.Abstractions
{
    public interface IServicesRepository
    {
        Task<List<Service>> GetAllAsync(CancellationToken ct = default);
        Task<Service?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string serviceCode, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(Service service, CancellationToken ct = default);
        Task UpdateAsync(Service service, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}
