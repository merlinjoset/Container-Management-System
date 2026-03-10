namespace ContainerManagement.Application.Dtos.Schedules;

public class ScheduleRowDto
{
    public string? Service { get; set; }
    public string? VesselCode { get; set; }
    public string? VesselName { get; set; }
    public string? Voyage { get; set; }
    public string? Bound { get; set; }
    public string? POL { get; set; }
    public string? POLTerminal { get; set; }
    public string? POD { get; set; }
    public string? PODTerminal { get; set; }
    public DateTime? ETD { get; set; }
    public DateTime? ETA { get; set; }
    public decimal? TransitDays { get; set; }
}
