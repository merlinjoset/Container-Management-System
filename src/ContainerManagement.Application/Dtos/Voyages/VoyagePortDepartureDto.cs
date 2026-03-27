namespace ContainerManagement.Application.Dtos.Voyages;

public class VoyagePortDepartureDto
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
    public string? NextPortCode { get; set; }
    public DateTime? ETANextPort { get; set; }
    public string? TugsOut { get; set; }
    public decimal? DepDraftFwdMtr { get; set; }
    public decimal? DepDraftAftMtr { get; set; }
    public decimal? DepDraftMeanMtr { get; set; }
    public decimal? FuelOil { get; set; }
    public decimal? DieselOil { get; set; }
    public decimal? FreshWater { get; set; }
    public decimal? BallastWater { get; set; }
    public string? Remarks { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
