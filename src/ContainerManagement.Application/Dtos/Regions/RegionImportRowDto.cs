namespace ContainerManagement.Application.Dtos.Regions
{
    public class RegionImportRowDto
    {
        public int RowNumber { get; set; }
        public string? RegionName { get; set; }
        public string? RegionCode { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
