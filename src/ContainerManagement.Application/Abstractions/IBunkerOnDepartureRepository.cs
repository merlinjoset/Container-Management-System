using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface IBunkerOnDepartureRepository
{
    Task<List<BunkerOnDeparture>> GetByDepartureIdAsync(Guid departureId, CancellationToken ct = default);
    Task ReplaceForDepartureAsync(Guid departureId, List<BunkerOnDeparture> bunkers, CancellationToken ct = default);
}
