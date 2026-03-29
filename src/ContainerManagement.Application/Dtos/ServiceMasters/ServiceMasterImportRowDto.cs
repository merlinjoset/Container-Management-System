namespace ContainerManagement.Application.Dtos.ServiceMasters
{
    public class ServiceMasterImportRowDto
    {
        public int RowNumber { get; set; }
        public string? ServiceName { get; set; }
        public string? ServiceCode { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
