namespace ContainerManagement.Application.Dtos.RouteMasters
{
    public class RouteMasterImportRowDto
    {
        public int RowNumber { get; set; }
        public string? RouteName { get; set; }
        public string? OriginCode { get; set; }
        public Guid? OriginPortId { get; set; }
        public string? DestCode { get; set; }
        public Guid? DestPortId { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
