using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Vendors
{
    public class VendorUpdateDto
    {
        public Guid Id { get; set; }

        [Required, StringLength(150)]
        public string VendorName { get; set; }

        [Required, StringLength(50)]
        public string VendorCode { get; set; }

        [Required]
        public Guid CountryId { get; set; }

        public Guid ModifiedBy { get; set; }
    }
}

