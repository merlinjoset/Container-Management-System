using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Services
{
    public class Service : AuditableEntity
    {
        public Guid Id { get; set; }
        public string? ServiceCode { get; set; }
        public string? ServiceName { get; set; }
    }
}
