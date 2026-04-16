namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class TosEntity
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

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
