namespace ContainerManagement.Application.Dtos.DistanceMasters
{
    public class DistanceMasterImportRowDto
    {
        public int RowNumber { get; set; }
        public string? FromPortCode { get; set; }
        public Guid? FromPortId { get; set; }
        public string? ToPortCode { get; set; }
        public Guid? ToPortId { get; set; }
        public decimal? Distance { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
