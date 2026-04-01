using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface IBunkerOnArrivalRepository
{
    Task<List<BunkerOnArrival>> GetByArrivalIdAsync(Guid arrivalId, CancellationToken ct = default);
    Task ReplaceForArrivalAsync(Guid arrivalId, List<BunkerOnArrival> bunkers, CancellationToken ct = default);
}
