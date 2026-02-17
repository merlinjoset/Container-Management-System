namespace ContainerManagement.Application.Dtos.Ports
{
    public class PortUpdateDto
    {
        public Guid Id { get; set; }
        public string PortCode { get; set; }
        public string FullName { get; set; }
        public string Country { get; set; }
        public string? Region { get; set; }
        public string? RegionCode { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
