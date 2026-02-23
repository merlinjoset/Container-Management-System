namespace ContainerManagement.Application.Dtos.Countries
{
    public class CountryCreateDto
    {
        public string? CountryName { get; set; }
        public string? CountryCode { get; set; }
        public Guid CreatedBy { get; set; }
    }
}

