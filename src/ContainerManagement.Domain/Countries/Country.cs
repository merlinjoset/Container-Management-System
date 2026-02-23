using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Countries
{
    public class Country : AuditableEntity
    {
        public Guid Id { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
    }
}

