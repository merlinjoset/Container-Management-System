namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class VendorEntity
    {
        public Guid Id { get; set; }
        public string VendorName { get; set; }
        public string VendorCode { get; set; }
        public Guid CountryId { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}

