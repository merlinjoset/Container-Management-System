namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class BunkerSupplyEntity
{
    public Guid Id { get; set; }
    public Guid DepartureId { get; set; }
    public string FuelType { get; set; } = string.Empty;
    public decimal? Qty { get; set; }
    public decimal? RateMts { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
