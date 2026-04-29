using ContainerManagement.Domain.Voyages;

namespace ContainerManagement.Application.Abstractions;

public interface IEditRequestRepository
{
    Task<Guid> AddAsync(EditRequest request, CancellationToken ct = default);
    Task<List<EditRequest>> GetPendingAsync(CancellationToken ct = default);
    Task<EditRequest?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task UpdateStatusAsync(Guid id, string status, Guid approvedBy, CancellationToken ct = default);
}
