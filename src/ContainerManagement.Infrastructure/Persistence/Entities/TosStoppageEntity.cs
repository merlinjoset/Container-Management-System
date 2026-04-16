namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class TosStoppageEntity
{
    public Guid Id { get; set; }
    public Guid TosId { get; set; }
    public int RowNumber { get; set; }
    public DateTime? FromDateTime { get; set; }
    public DateTime? ToDateTime { get; set; }
    public decimal? NonProductiveHours { get; set; }
    public string? Reason { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
