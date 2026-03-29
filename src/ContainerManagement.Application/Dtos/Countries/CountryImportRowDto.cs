namespace ContainerManagement.Application.Dtos.Countries
{
    public class CountryImportRowDto
    {
        public int RowNumber { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
