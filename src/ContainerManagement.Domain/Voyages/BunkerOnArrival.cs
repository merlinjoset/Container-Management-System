using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class BunkerOnArrival : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid ArrivalId { get; set; }
    /// <summary>
    /// Reading point: "ArrivalTime", "PilotOnBoard", "ArrivalBerth"
    /// </summary>
    public string ReadingPoint { get; set; } = string.Empty;
    public decimal? VlsfoMts { get; set; }
    public decimal? MgoMts { get; set; }
    public decimal? HfoMts { get; set; }
}
