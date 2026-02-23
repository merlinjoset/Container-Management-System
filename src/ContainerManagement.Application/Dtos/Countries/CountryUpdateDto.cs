namespace ContainerManagement.Application.Dtos.Countries
{
    public class CountryUpdateDto
    {
        public Guid Id { get; set; }
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}

