namespace ContainerManagement.Application.Dtos.Regions
{
    public class RegionUpdateDto
    {
        public Guid Id { get; set; }
        public string? RegionName { get; set; }
        public string? RegionCode { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}

