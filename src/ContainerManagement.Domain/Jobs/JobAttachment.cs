using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Jobs
{
    public class JobAttachment : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsScreenshot { get; set; }
    }
}
