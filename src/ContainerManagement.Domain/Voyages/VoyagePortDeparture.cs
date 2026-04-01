using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class VoyagePortDeparture : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid VoyagePortId { get; set; }
    public string? InboundVoyage { get; set; }
    public string? OutboundVoyage { get; set; }
    public DateTime? CompleteCargoOperation { get; set; }
    public DateTime? PilotOnBoard { get; set; }
    public DateTime? UnberthFAOP { get; set; }
    public DateTime? ActualETD { get; set; }
    public Guid? NextPortId { get; set; }
    public DateTime? ETANextPort { get; set; }
    public int? TugsOut { get; set; }
    public decimal? DepDraftFwdMtr { get; set; }
    public decimal? DepDraftAftMtr { get; set; }
    public decimal? DepDraftMeanMtr { get; set; }
    public decimal? FreshWater { get; set; }
    public decimal? BallastWater { get; set; }
    public string? Remarks { get; set; }
}
