namespace ContainerManagement.Application.Dtos.Voyages;

public class BunkerSupplyDto
{
    public string FuelType { get; set; } = string.Empty;
    public decimal? Qty { get; set; }
    public decimal? RateMts { get; set; }
}
