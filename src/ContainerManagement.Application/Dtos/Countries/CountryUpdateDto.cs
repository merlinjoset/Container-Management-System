using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Countries
{
    public class CountryUpdateDto
    {
        public Guid Id { get; set; }
        [StringLength(100)]
        public string? CountryName { get; set; }
        [StringLength(20)]
        public string? CountryCode { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
