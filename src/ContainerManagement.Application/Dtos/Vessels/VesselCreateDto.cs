using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Vessels
{
    public class VesselCreateDto
    {
        [Required, StringLength(150)]
        public string VesselName { get; set; }

        [Required, StringLength(50)]
        public string VesselCode { get; set; }

        [Required, StringLength(20)]
        public string ImoCode { get; set; }

        public int? Teus { get; set; }
        public decimal? NRT { get; set; }
        public decimal? GRT { get; set; }

        [StringLength(100)]
        public string? Flag { get; set; }
        public decimal? Speed { get; set; }
        public int? BuildYear { get; set; }

        public Guid CreatedBy { get; set; }
    }
}

