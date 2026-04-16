namespace ContainerManagement.Application.Dtos.Voyages;

public class TosStoppageDto
{
    public Guid Id { get; set; }
    public Guid? TosId { get; set; }
    public int RowNumber { get; set; }
    public DateTime? FromDateTime { get; set; }
    public DateTime? ToDateTime { get; set; }
    public decimal? NonProductiveHours { get; set; }
    public string? Reason { get; set; }
}
