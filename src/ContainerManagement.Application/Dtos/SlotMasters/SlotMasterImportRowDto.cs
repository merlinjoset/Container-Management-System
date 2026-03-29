namespace ContainerManagement.Application.Dtos.SlotMasters
{
    public class SlotMasterImportRowDto
    {
        public int RowNumber { get; set; }
        public string? SlotName { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
