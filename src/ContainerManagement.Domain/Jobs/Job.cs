using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Jobs
{
    public class Job : AuditableEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public JobStatus Status { get; set; } = JobStatus.ToDo;
        public string? Tag { get; set; }
        public string? TagColor { get; set; }
        public DateTime? CompletedDate { get; set; }
    }
}
