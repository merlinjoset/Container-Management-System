using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Ports
{
    public class PortCreateDto
    {
        [Required]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Port Code must be exactly 5 characters.")]
        public string PortCode { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public string? Region { get; set; }
        public string? RegionCode { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
