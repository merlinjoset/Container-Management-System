using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class Tos : AuditableEntity
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
}
