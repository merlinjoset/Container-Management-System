using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Terminals
{
    public class Terminal : AuditableEntity
    {
        public Guid Id { get; set; }
        public Guid PortId { get; set; }
        public string TerminalName { get; set; }
        public string TerminalCode { get; set; }
    }
}

