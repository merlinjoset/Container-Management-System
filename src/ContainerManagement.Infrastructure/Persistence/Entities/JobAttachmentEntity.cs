namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class JobAttachmentEntity
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsScreenshot { get; set; }
        public byte[]? FileData { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
