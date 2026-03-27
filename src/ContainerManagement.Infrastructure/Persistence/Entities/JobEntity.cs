namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class JobEntity
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int Status { get; set; }
        public string? Tag { get; set; }
        public string? TagColor { get; set; }
        public DateTime? CompletedDate { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
