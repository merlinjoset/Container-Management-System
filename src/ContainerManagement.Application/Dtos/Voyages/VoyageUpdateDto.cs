namespace ContainerManagement.Application.Dtos.Voyages;

public class VoyageUpdateDto
{
    public Guid Id { get; set; }
    public Guid VesselId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? OperatorId { get; set; }
    public string? VoyageType { get; set; }
    public Guid ModifiedBy { get; set; }
    public List<VoyagePortCreateDto> Ports { get; set; } = new();
}
