namespace ContainerManagement.Application.Dtos.Vendors
{
    public class VendorImportRowDto
    {
        public int RowNumber { get; set; }
        public string? VendorName { get; set; }
        public string? VendorCode { get; set; }
        public string? CountryCode { get; set; }
        public Guid? CountryId { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
