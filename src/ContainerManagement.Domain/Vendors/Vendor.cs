using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Vendors
{
    public class Vendor : AuditableEntity
    {
        public Guid Id { get; set; }
        public string VendorName { get; set; }
        public string VendorCode { get; set; }
        public Guid CountryId { get; set; }
    }
}

