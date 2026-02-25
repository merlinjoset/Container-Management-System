using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Ports
{
    public class Port : AuditableEntity
    {
        public Guid Id { get; set; }
        public string PortCode { get; set; }
        public string FullName { get; set; }
        public Guid CountryId { get; set; }
        public Guid RegionId { get; set; }
        public string? RegionCode { get; set; }
    }
}
