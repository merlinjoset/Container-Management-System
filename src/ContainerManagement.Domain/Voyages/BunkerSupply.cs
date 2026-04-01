using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class BunkerSupply : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid DepartureId { get; set; }
    /// <summary>
    /// Fuel type: "VLSFO", "MGO", "HFO"
    /// </summary>
    public string FuelType { get; set; } = string.Empty;
    public decimal? Qty { get; set; }
    public decimal? RateMts { get; set; }
}
