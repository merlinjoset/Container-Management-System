using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface ITosRepository
{
    Task<Tos?> GetByVoyagePortIdAsync(Guid voyagePortId, CancellationToken ct = default);
    Task AddAsync(Tos tos, CancellationToken ct = default);
    Task UpdateAsync(Tos tos, CancellationToken ct = default);
}
