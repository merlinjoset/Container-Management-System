namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class VesselEntity
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

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}

