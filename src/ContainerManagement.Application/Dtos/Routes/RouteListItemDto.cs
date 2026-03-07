namespace ContainerManagement.Application.Dtos.Routes
{
    public class RouteListItemDto
    {
        public Guid Id { get; set; }
        public string? RouteName { get; set; }
        public Guid PortOfOriginId { get; set; }
        public Guid FinalDestinationId { get; set; }
        public string? PortOfOriginName { get; set; }
        public string? FinalDestinationName { get; set; }
    }
}
