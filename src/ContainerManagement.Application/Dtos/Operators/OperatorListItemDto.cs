namespace ContainerManagement.Application.Dtos.Operators
{
    public class OperatorListItemDto
    {
        public Guid Id { get; set; }
        public string OperatorName { get; set; }
        // Unique code removed
        public Guid VendorId { get; set; }
        public Guid CountryId { get; set; }
        public string? VendorName { get; set; }
        public string? CountryName { get; set; }
    }
}
