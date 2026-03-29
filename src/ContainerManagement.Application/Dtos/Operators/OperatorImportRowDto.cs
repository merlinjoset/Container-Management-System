namespace ContainerManagement.Application.Dtos.Operators
{
    public class OperatorImportRowDto
    {
        public int RowNumber { get; set; }
        public string? OperatorName { get; set; }
        public string? VendorCode { get; set; }
        public Guid? VendorId { get; set; }
        public string? IsCompetitorText { get; set; }
        public bool IsCompetitor { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
