namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class PortsEntity
    {
        public Guid Id { get; set; }
        public string? PortCode { get; set; }
        public string? FullName { get; set; }
        public Guid CountryId { get; set; }
        public Guid RegionId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
