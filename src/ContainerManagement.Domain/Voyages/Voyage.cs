using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class Voyage : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid VesselId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? OperatorId { get; set; }
    public string? VoyageType { get; set; }   // Own, Third Party, Dead Freight, Slot Swap, Vessel Swap
}
