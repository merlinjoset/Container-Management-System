using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface ITosSummaryRepository
{
    Task<TosSummary?> GetByTosIdAsync(Guid tosId, CancellationToken ct = default);
    Task UpsertAsync(TosSummary summary, CancellationToken ct = default);
}
