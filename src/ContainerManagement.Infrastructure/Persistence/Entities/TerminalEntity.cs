namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class TerminalEntity
    {
        public Guid Id { get; set; }
        public Guid PortId { get; set; }
        public string TerminalName { get; set; }
        public string TerminalCode { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}

