using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface IVoyagePortsRepository
{
    Task<List<VoyagePort>> GetByVoyageIdAsync(Guid voyageId, CancellationToken ct = default);
    Task AddAsync(VoyagePort port, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<VoyagePort> ports, CancellationToken ct = default);
    Task DeleteByVoyageIdAsync(Guid voyageId, CancellationToken ct = default);
    Task UpdateETDAsync(Guid voyagePortId, DateTime etd, Guid modifiedBy, CancellationToken ct = default);
}
