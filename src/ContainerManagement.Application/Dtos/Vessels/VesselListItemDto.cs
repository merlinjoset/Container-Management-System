namespace ContainerManagement.Application.Dtos.Vessels
{
    public class VesselListItemDto
    {
        public Guid Id { get; set; }
        public string VesselName { get; set; }
        public string VesselCode { get; set; }
        public string ImoCode { get; set; }
        public int? Teus { get; set; }
        public decimal? NRT { get; set; }
        public decimal? GRT { get; set; }
        public string? Flag { get; set; }
        public decimal? Speed { get; set; }
        public int? BuildYear { get; set; }
    }
}

