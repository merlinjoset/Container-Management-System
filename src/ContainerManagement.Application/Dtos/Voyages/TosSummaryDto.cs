namespace ContainerManagement.Application.Dtos.Voyages;

public class TosSummaryDto
{
    public Guid Id { get; set; }
    public decimal? VesselTurnaroundTime { get; set; }
    public decimal? BerthingDelay { get; set; }
    public int? TotalMoves { get; set; }
    public decimal? NonProductiveTime { get; set; }
    public decimal? TerminalProductivity { get; set; }
    public decimal? ShipProductivity { get; set; }
}
