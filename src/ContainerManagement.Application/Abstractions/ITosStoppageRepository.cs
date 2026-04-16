using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface ITosStoppageRepository
{
    Task<List<TosStoppage>> GetByTosIdAsync(Guid tosId, CancellationToken ct = default);
    Task ReplaceForTosAsync(Guid tosId, List<TosStoppage> stoppages, CancellationToken ct = default);
}
