namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class RegionEntity
    {
        public Guid Id { get; set; }
        public string? RegionName { get; set; }
        public string? RegionCode { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}

