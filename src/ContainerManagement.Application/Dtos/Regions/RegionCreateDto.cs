using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Regions
{
    public class RegionCreateDto
    {
        [StringLength(100)]
        public string? RegionName { get; set; }
        [StringLength(20)]
        public string? RegionCode { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
