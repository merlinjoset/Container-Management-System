using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Ports
{
    public class PortCreateDto
    {
        [Required]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Port Code must be exactly 5 characters.")]
        public string PortCode { get; set; }
        [Required]
        [StringLength(200)]
        public string FullName { get; set; }
        [Required]
        public Guid CountryId { get; set; }
        [Required]
        public Guid RegionId { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
