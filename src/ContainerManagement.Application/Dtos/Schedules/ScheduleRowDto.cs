namespace ContainerManagement.Application.Dtos.Schedules;

public class ScheduleRowDto
{
    public Guid VoyageId { get; set; }
    public string? Service { get; set; }
    public string? VesselCode { get; set; }
    public string? VesselName { get; set; }
    public string? Operator { get; set; }
    public string? Slot { get; set; }
    public string? Voyage { get; set; }
    public string? Bound { get; set; }
    public int Leg { get; set; }
    public int TotalLegs { get; set; }
    public string? Port { get; set; }
    public string? Terminal { get; set; }
    public DateTime? ETA { get; set; }
    public DateTime? ETB { get; set; }
    public DateTime? ETD { get; set; }
    public int? PortStay { get; set; }
    public decimal? SeaDay { get; set; }
    public decimal? Speed { get; set; }
    public decimal? Distance { get; set; }
}
