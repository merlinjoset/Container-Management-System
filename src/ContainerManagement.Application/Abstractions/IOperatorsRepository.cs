using ContainerManagement.Domain.Operators;

namespace ContainerManagement.Application.Abstractions
{
    public interface IOperatorsRepository
    {
        Task<List<Operator>> GetAllAsync(CancellationToken ct = default);
        Task<Operator?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Operator op, CancellationToken ct = default);
        Task UpdateAsync(Operator op, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}
