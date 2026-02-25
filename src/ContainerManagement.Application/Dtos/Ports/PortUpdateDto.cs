using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Ports
{
    public class PortUpdateDto
    {
        public Guid Id { get; set; }
        [Required]
        [StringLength(5, MinimumLength = 5, ErrorMessage = "Port Code must be exactly 5 characters.")]
        public string PortCode { get; set; }
        public string FullName { get; set; }
        public Guid CountryId { get; set; }
        public Guid RegionId { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
