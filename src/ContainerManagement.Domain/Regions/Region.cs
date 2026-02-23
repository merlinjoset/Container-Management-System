using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Regions
{
    public class Region : AuditableEntity
    {
        public Guid Id { get; set; }
        public string? RegionName { get; set; }
        public string? RegionCode { get; set; }
    }
}

