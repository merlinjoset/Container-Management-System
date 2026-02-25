namespace ContainerManagement.Application.Dtos.Vendors
{
    public class VendorListItemDto
    {
        public Guid Id { get; set; }
        public string VendorName { get; set; }
        public string VendorCode { get; set; }
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }
    }
}

