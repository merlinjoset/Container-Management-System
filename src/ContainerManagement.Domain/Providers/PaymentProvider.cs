namespace ContainerManagement.Domain.Providers;

public class PaymentProvider
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public string EndpointUrl { get; set; } = default!;
}