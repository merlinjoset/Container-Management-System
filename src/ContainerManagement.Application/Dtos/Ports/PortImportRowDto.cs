namespace ContainerManagement.Application.Dtos.Ports
{
    public class PortImportRowDto
    {
        public int RowNumber { get; set; }
        public string? PortCode { get; set; }
        public string? FullName { get; set; }
        public string? CountryCode { get; set; }
        public Guid? CountryId { get; set; }
        public string? RegionCode { get; set; }
        public Guid? RegionId { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
