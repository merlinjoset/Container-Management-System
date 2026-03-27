namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class VoyagePortArrivalEntity
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
    public string? TugsIn { get; set; }
    public decimal? ArrivalDraftFwdMtr { get; set; }
    public decimal? ArrivalDraftAftMtr { get; set; }
    public decimal? ArrivalDraftMeanMtr { get; set; }
    public decimal? FuelOil { get; set; }
    public decimal? DieselOil { get; set; }
    public decimal? FreshWater { get; set; }
    public decimal? BallastWater { get; set; }
    public string? Remarks { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
