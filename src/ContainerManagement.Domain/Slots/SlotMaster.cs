using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Slots
{
    public class SlotMaster : AuditableEntity
    {
        public Guid Id { get; set; }
        public string? SlotName { get; set; }
    }
}
