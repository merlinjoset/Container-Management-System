using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Countries
{
    public class CountryCreateDto
    {
        [StringLength(100)]
        public string? CountryName { get; set; }
        [StringLength(20)]
        public string? CountryCode { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
