namespace ContainerManagement.Application.Dtos.Voyages;

public class ShipProductivityDto
{
    public Guid Id { get; set; }
    public decimal? PortStayTime { get; set; }
    public decimal? ProductivityPerHr { get; set; }
    public decimal? MovesPerCrane { get; set; }
}
