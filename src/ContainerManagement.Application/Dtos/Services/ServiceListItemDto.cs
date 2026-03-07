namespace ContainerManagement.Application.Dtos.Services
{
    public class ServiceListItemDto
    {
        public Guid Id { get; set; }
        public string? ServiceCode { get; set; }
        public string? ServiceName { get; set; }
    }
}
