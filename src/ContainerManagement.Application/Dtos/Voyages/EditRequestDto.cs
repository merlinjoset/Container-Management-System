namespace ContainerManagement.Application.Dtos.Voyages;

public class EditRequestDto
{
    public Guid VoyagePortId { get; set; }
    public string ReportType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}
