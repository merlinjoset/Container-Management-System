namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class ServiceEntity
    {
        public Guid Id { get; set; }
        public string? ServiceCode { get; set; }
        public string? ServiceName { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
