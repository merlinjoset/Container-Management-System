using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class TosSummary : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid TosId { get; set; }
    public decimal? VesselTurnaroundTime { get; set; }
    public decimal? BerthingDelay { get; set; }
    public int? TotalMoves { get; set; }
    public decimal? NonProductiveTime { get; set; }
    public decimal? TerminalProductivity { get; set; }
    public decimal? ShipProductivity { get; set; }
}
