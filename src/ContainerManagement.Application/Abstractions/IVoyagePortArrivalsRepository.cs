using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface IVoyagePortArrivalsRepository
{
    Task<VoyagePortArrival?> GetByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default);
    Task<VoyagePortArrival?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(VoyagePortArrival arrival, CancellationToken ct = default);
    Task UpdateAsync(VoyagePortArrival arrival, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
}
