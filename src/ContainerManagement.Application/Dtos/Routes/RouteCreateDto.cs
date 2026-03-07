using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Routes
{
    public class RouteCreateDto
    {
        [StringLength(150)]
        public string? RouteName { get; set; }
        public Guid PortOfOriginId { get; set; }
        public Guid FinalDestinationId { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
