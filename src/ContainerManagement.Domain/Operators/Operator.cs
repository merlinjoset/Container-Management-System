using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Operators
{
    public class Operator : AuditableEntity
    {
        public Guid Id { get; set; }
        public string OperatorName { get; set; }
        public Guid VendorId { get; set; }
        public Guid CountryId { get; set; }
    }
}
