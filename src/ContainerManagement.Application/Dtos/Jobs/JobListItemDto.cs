namespace ContainerManagement.Application.Dtos.Jobs
{
    public class JobListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Status { get; set; }
        public string? Tag { get; set; }
        public string? TagColor { get; set; }
        public DateTime? CompletedDate { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public List<JobAttachmentDto> Attachments { get; set; } = new();
    }
}
