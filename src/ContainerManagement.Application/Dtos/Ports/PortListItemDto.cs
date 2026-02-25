namespace ContainerManagement.Application.Dtos.Ports
{
    public class PortListItemDto
    {
        public Guid Id { get; set; }
        public string PortCode { get; set; }
        public string FullName { get; set; }
        public Guid CountryId { get; set; }
        public Guid RegionId { get; set; }
        public string? CountryName { get; set; }
        public string? RegionName { get; set; }
    }
}
