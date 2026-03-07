using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Routes
{
    public class RouteUpdateDto
    {
        public Guid Id { get; set; }
        [StringLength(150)]
        public string? RouteName { get; set; }
        public Guid PortOfOriginId { get; set; }
        public Guid FinalDestinationId { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
