namespace ContainerManagement.Application.Dtos.Jobs
{
    public class JobAttachmentDto
    {
        public Guid Id { get; set; }
        public Guid JobId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public bool IsScreenshot { get; set; }
        public DateTime CreatedOn { get; set; }
        public string Url { get; set; } = string.Empty;
    }
}
