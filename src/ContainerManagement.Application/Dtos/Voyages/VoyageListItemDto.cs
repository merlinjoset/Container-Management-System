namespace ContainerManagement.Application.Dtos.Voyages;

public class VoyageListItemDto
{
    public Guid Id { get; set; }
    public Guid VesselId { get; set; }
    public string? VesselName { get; set; }
    public Guid? ServiceId { get; set; }
    public string? ServiceCode { get; set; }
    public string? ServiceName { get; set; }
    public Guid? OperatorId { get; set; }
    public string? OperatorName { get; set; }
    public string? VoyageType { get; set; }
    public DateTime CreatedOn { get; set; }
}
