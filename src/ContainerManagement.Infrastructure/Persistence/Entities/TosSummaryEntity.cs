namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class TosSummaryEntity
{
    public Guid Id { get; set; }
    public Guid TosId { get; set; }
    public decimal? VesselTurnaroundTime { get; set; }
    public decimal? BerthingDelay { get; set; }
    public int? TotalMoves { get; set; }
    public decimal? NonProductiveTime { get; set; }
    public decimal? TerminalProductivity { get; set; }
    public decimal? ShipProductivity { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
