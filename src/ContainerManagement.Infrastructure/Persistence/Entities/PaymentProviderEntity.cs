namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class PaymentProviderEntity
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public bool IsActive { get; set; }
    public string? EndpointUrl { get; set; }
}
