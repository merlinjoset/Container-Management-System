using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface IVoyagesRepository
{
    Task<List<Voyage>> GetAllAsync(CancellationToken ct = default);
    Task<Voyage?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task AddAsync(Voyage voyage, CancellationToken ct = default);
    Task UpdateAsync(Voyage voyage, CancellationToken ct = default);
    Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
}
