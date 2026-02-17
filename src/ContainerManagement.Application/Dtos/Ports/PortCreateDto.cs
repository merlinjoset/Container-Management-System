namespace ContainerManagement.Application.Dtos.Ports
{
    public class PortCreateDto
    {
        public string PortCode { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public string? Region { get; set; }
        public string? RegionCode { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
