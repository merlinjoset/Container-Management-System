using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Ports
{
    public class Port : AuditableEntity
    {
        public Guid Id { get; set; }
        public string PortCode { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public string? Region { get; set; }
        public string? RegionCode { get; set; }
    }
}
