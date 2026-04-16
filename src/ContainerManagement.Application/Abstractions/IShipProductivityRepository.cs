using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface IShipProductivityRepository
{
    Task<ShipProductivity?> GetByTosIdAsync(Guid tosId, CancellationToken ct = default);
    Task UpsertAsync(ShipProductivity sp, CancellationToken ct = default);
}
