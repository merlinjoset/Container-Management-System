using ContainerManagement.Domain.Terminals;

namespace ContainerManagement.Application.Abstractions
{
    public interface ITerminalsRepository
    {
        Task<List<Terminal>> GetAllAsync(CancellationToken ct = default);
        Task<Terminal?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string terminalCode, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(Terminal terminal, CancellationToken ct = default);
        Task UpdateAsync(Terminal terminal, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}

