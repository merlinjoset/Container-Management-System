using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Routes
{
    public class Route : AuditableEntity
    {
        public Guid Id { get; set; }
        public string? RouteName { get; set; }
        public Guid PortOfOriginId { get; set; }
        public Guid FinalDestinationId { get; set; }
    }
}
