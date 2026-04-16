using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface ICraneProductivityRepository
{
    Task<CraneProductivity?> GetByTosIdAsync(Guid tosId, CancellationToken ct = default);
    Task UpsertAsync(CraneProductivity cp, CancellationToken ct = default);
}
