using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class TugUsage : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid? ArrivalId { get; set; }
    public Guid? DepartureId { get; set; }
    public int TugNumber { get; set; }       // 1, 2, 3, 4, 5
    public decimal? Hours { get; set; }
}
