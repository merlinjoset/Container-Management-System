using ContainerManagement.Domain.Jobs;

namespace ContainerManagement.Application.Abstractions
{
    public interface IJobsRepository
    {
        Task<List<Job>> GetAllAsync(CancellationToken ct = default);
        Task<Job?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(Job job, CancellationToken ct = default);
        Task UpdateAsync(Job job, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}
