using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Distances
{
    public class DistanceMaster : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid FromPortId { get; set; }
        public Guid ToPortId { get; set; }
        public decimal Distance { get; set; }
    }
}
