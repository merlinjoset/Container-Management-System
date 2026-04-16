namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class CraneProductivityEntity
{
    public Guid Id { get; set; }
    public Guid TosId { get; set; }
    public int? CranesPerSI { get; set; }
    public int? ActualCranes { get; set; }
    public decimal? TotalOpsTime { get; set; }
    public decimal? NonProductiveTime { get; set; }
    public decimal? CraneWorkingHrs { get; set; }
    public decimal? CraneProductivityPerHr { get; set; }
    public decimal? MovesPerCrane { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
