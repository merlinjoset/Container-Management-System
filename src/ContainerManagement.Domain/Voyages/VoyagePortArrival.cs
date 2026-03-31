using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class VoyagePortArrival : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid VoyagePortId { get; set; }
    public string? InboundVoyage { get; set; }
    public string? OutboundVoyage { get; set; }
    public DateTime? ActualETA { get; set; }
    public DateTime? ActualETB { get; set; }
    public Guid? LastPortId { get; set; }
    public Guid? NextPortId { get; set; }
    public DateTime? PilotOnBoard { get; set; }
    public DateTime? CommencedCargoOperation { get; set; }
    public int? TugsIn { get; set; }
    public decimal? ArrivalDraftFwdMtr { get; set; }
    public decimal? ArrivalDraftAftMtr { get; set; }
    public decimal? ArrivalDraftMeanMtr { get; set; }
    public decimal? FuelOil { get; set; }
    public decimal? DieselOil { get; set; }
    public decimal? FreshWater { get; set; }
    public decimal? BallastWater { get; set; }
    public string? Remarks { get; set; }
}
