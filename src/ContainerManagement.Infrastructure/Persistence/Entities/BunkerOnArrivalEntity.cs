namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class BunkerOnArrivalEntity
{
    public Guid Id { get; set; }
    public Guid ArrivalId { get; set; }
    public string ReadingPoint { get; set; } = string.Empty;
    public decimal? VlsfoMts { get; set; }
    public decimal? MgoMts { get; set; }
    public decimal? HfoMts { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
