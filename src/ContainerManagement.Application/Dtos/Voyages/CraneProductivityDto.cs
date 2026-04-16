namespace ContainerManagement.Application.Dtos.Voyages;

public class CraneProductivityDto
{
    public Guid Id { get; set; }
    public int? CranesPerSI { get; set; }
    public int? ActualCranes { get; set; }
    public decimal? TotalOpsTime { get; set; }
    public decimal? NonProductiveTime { get; set; }
    public decimal? CraneWorkingHrs { get; set; }
    public decimal? CraneProductivityPerHr { get; set; }
    public decimal? MovesPerCrane { get; set; }
}
