namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class VoyagePortEntity
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
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
