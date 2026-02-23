namespace ContainerManagement.Application.Dtos.Regions
{
    public class RegionCreateDto
    {
        public string? RegionName { get; set; }
        public string? RegionCode { get; set; }
        public Guid CreatedBy { get; set; }
    }
}

