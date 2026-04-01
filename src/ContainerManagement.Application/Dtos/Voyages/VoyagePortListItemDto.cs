namespace ContainerManagement.Application.Dtos.Voyages;

public class VoyagePortListItemDto
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string? VoyNo { get; set; }
    public string? Bound { get; set; }
    public Guid PortId { get; set; }
    public string? PortCode { get; set; }
    public Guid? TerminalId { get; set; }
    public string? TerminalCode { get; set; }
    public DateTime? ETA { get; set; }
    public DateTime? ETB { get; set; }
    public DateTime? ETD { get; set; }
    public int? PortStay { get; set; }
    public decimal? SeaDay { get; set; }
    public decimal? Speed { get; set; }
    public decimal? Distance { get; set; }
    public int SortOrder { get; set; }
    public bool HasArrival { get; set; }
    public bool HasDeparture { get; set; }

    // Actual dates from Arrival/Departure reports
    public DateTime? ActualETA { get; set; }
    public DateTime? ActualETB { get; set; }
    public DateTime? ActualETD { get; set; }

    // Arrival report fields
    public DateTime? ArrPilotOnBoard { get; set; }
    public DateTime? ArrCommencedCargoOp { get; set; }
    public int? ArrTugsIn { get; set; }
    public decimal? ArrDraftFwd { get; set; }
    public decimal? ArrDraftAft { get; set; }
    public decimal? ArrDraftMean { get; set; }
    public decimal? ArrFreshWater { get; set; }
    public decimal? ArrBallastWater { get; set; }
    public string? ArrRemarks { get; set; }

    // Departure report fields
    public DateTime? DepCompleteCargoOp { get; set; }
    public DateTime? DepPilotOnBoard { get; set; }
    public DateTime? DepUnberthFAOP { get; set; }
    public int? DepTugsOut { get; set; }
    public decimal? DepDraftFwd { get; set; }
    public decimal? DepDraftAft { get; set; }
    public decimal? DepDraftMean { get; set; }
    public decimal? DepFreshWater { get; set; }
    public decimal? DepBallastWater { get; set; }
    public string? DepRemarks { get; set; }
}
