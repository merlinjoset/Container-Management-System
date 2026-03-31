namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class TugUsageEntity
{
    public Guid Id { get; set; }
    public Guid? ArrivalId { get; set; }
    public Guid? DepartureId { get; set; }
    public int TugNumber { get; set; }
    public decimal? Hours { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
