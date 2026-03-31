using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface ITugUsageRepository
{
    Task<List<TugUsage>> GetByArrivalIdAsync(Guid arrivalId, CancellationToken ct = default);
    Task<List<TugUsage>> GetByDepartureIdAsync(Guid departureId, CancellationToken ct = default);
    Task ReplaceForArrivalAsync(Guid arrivalId, List<TugUsage> tugs, CancellationToken ct = default);
    Task ReplaceForDepartureAsync(Guid departureId, List<TugUsage> tugs, CancellationToken ct = default);
}
