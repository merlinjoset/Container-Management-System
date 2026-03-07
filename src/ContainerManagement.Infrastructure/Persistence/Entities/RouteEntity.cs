namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class RouteEntity
    {
        public Guid Id { get; set; }
        public string? RouteName { get; set; }
        public Guid PortOfOriginId { get; set; }
        public Guid FinalDestinationId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
