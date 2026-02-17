using ContainerManagement.Domain.Ports;

namespace ContainerManagement.Application.Abstractions
{
    public interface IPortsRepository
    {
        Task<List<Port>> GetAllAsync(CancellationToken ct = default);
        Task<Port?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string portCode, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(Port port, CancellationToken ct = default);
        Task UpdateAsync(Port port, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}
