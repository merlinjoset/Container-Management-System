using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class BunkerOnDeparture : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid DepartureId { get; set; }
    /// <summary>
    /// Reading point: "CompleteCargo", "PilotOnBoard", "Departure"
    /// </summary>
    public string ReadingPoint { get; set; } = string.Empty;
    public decimal? VlsfoMts { get; set; }
    public decimal? MgoMts { get; set; }
    public decimal? HfoMts { get; set; }
}
