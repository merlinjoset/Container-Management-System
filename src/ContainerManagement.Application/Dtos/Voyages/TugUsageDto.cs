namespace ContainerManagement.Application.Dtos.Voyages;

public class TugUsageDto
{
    public Guid Id { get; set; }
    public int TugNumber { get; set; }
    public decimal? Hours { get; set; }
}
