using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Application.Dtos.Vessels
{
    public class VesselCreateDto
    {
        [Required, StringLength(150)]
        [Display(Name = "Vessel Name")]
        public string VesselName { get; set; }

        [Required, StringLength(50)]
        [Display(Name = "Vessel Code")]
        public string VesselCode { get; set; }

        [Required, StringLength(20)]
        [Display(Name = "IMO Code")]
        public string ImoCode { get; set; }

        public int? Teus { get; set; }
        public decimal? NRT { get; set; }
        public decimal? GRT { get; set; }

        [StringLength(100)]
        public string? Flag { get; set; }
        public decimal? Speed { get; set; }
        
         [Display(Name = "Build Year")]
        public int? BuildYear { get; set; }

        public Guid CreatedBy { get; set; }
    }
}
