namespace ContainerManagement.Application.Dtos.Voyages;

public class VoyageCreateDto
{
    public Guid VesselId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? OperatorId { get; set; }
    public string? VoyageType { get; set; }
    public Guid CreatedBy { get; set; }
    public List<VoyagePortCreateDto> Ports { get; set; } = new();
}
