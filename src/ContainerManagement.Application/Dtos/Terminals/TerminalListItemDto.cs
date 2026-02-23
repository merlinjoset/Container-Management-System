namespace ContainerManagement.Application.Dtos.Terminals
{
    public class TerminalListItemDto
    {
        public Guid Id { get; set; }
        public Guid PortId { get; set; }
        public string TerminalName { get; set; }
        public string TerminalCode { get; set; }
        public string? PortCode { get; set; }
        public string? PortName { get; set; }
    }
}
