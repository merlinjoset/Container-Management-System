using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Regions
{
    public class RegionUpdateDto
    {
        public Guid Id { get; set; }
        [StringLength(100)]
        public string? RegionName { get; set; }
        [StringLength(20)]
        public string? RegionCode { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
