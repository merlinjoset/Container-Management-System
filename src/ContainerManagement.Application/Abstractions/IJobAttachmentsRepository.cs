using ContainerManagement.Domain.Jobs;

namespace ContainerManagement.Application.Abstractions
{
    public interface IJobAttachmentsRepository
    {
        Task<List<JobAttachment>> GetByJobIdAsync(Guid jobId, CancellationToken ct = default);
        Task<JobAttachment?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task AddAsync(JobAttachment attachment, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}
