using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class VoyagePort : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid VoyageId { get; set; }
    public string? VoyNo { get; set; }
    public string? Bound { get; set; }
    public Guid PortId { get; set; }
    public Guid? TerminalId { get; set; }
    public DateTime? ETA { get; set; }
    public DateTime? ETB { get; set; }
    public DateTime? ETD { get; set; }
    public int? PortStay { get; set; }
    public decimal? SeaDay { get; set; }
    public decimal? Speed { get; set; }
    public decimal? Distance { get; set; }
    public int SortOrder { get; set; }
}
