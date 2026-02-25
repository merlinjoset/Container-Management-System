using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Operators
{
    public class OperatorCreateDto
    {
        [Required, StringLength(150)]
        public string OperatorName { get; set; }

        // Unique code removed

        [Required]
        public Guid VendorId { get; set; }

        [Required]
        public Guid CountryId { get; set; }

        public Guid CreatedBy { get; set; }
    }
}
