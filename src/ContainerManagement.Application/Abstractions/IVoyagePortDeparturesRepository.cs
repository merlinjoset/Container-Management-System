using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface IVoyagePortDeparturesRepository
{
    Task<VoyagePortDeparture?> GetByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default);
    Task<VoyagePortDeparture?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(VoyagePortDeparture departure, CancellationToken ct = default);
    Task UpdateAsync(VoyagePortDeparture departure, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
}
