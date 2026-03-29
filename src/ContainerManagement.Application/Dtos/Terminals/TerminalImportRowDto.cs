namespace ContainerManagement.Application.Dtos.Terminals
{
    public class TerminalImportRowDto
    {
        public int RowNumber { get; set; }
        public string? TerminalName { get; set; }
        public string? TerminalCode { get; set; }
        public string? PortCode { get; set; }
        public Guid? PortId { get; set; }
        public List<string> Errors { get; set; } = new();
        public bool HasErrors => Errors.Count > 0;
    }
}
