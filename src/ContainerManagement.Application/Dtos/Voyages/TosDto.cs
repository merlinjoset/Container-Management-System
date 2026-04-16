namespace ContainerManagement.Application.Dtos.Voyages;

public class TosDto
{
    public Guid Id { get; set; }
    public Guid VoyagePortId { get; set; }
    // Container Moves
    public int? DischargeMoves { get; set; }
    public int? LoadMoves { get; set; }
    public int? Restows { get; set; }
    public int? TotalHatchCover { get; set; }
    public int? NoOfBin { get; set; }
    public int? TotalMoves { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }

    // Dynamic stoppage rows
    public List<TosStoppageDto> Stoppages { get; set; } = new();

    // Crane productivity (stored in TblCraneProductivity)
    public CraneProductivityDto? CraneProductivity { get; set; }

    // Ship productivity (stored in TblShipProductivity)
    public ShipProductivityDto? ShipProductivity { get; set; }

    // TOS summary (stored in TblTOSSummary)
    public TosSummaryDto? Summary { get; set; }
}
