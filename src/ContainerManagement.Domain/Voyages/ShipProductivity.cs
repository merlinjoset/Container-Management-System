using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class ShipProductivity : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid TosId { get; set; }
    public decimal? PortStayTime { get; set; }
    public decimal? ProductivityPerHr { get; set; }
    public decimal? MovesPerCrane { get; set; }
}
