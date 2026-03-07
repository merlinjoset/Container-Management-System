using ContainerManagement.Domain.Routes;

namespace ContainerManagement.Application.Abstractions
{
    public interface IRoutesRepository
    {
        Task<List<Route>> GetAllAsync(CancellationToken ct = default);
        Task<Route?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string routeName, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(Route route, CancellationToken ct = default);
        Task UpdateAsync(Route route, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}
