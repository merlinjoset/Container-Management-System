using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface IBunkerSupplyRepository
{
    Task<List<BunkerSupply>> GetByDepartureIdAsync(Guid departureId, CancellationToken ct = default);
    Task ReplaceForDepartureAsync(Guid departureId, List<BunkerSupply> supplies, CancellationToken ct = default);
}
