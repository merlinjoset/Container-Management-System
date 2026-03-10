namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class VoyageEntity
{
    public Guid Id { get; set; }
    public Guid VesselId { get; set; }
    public Guid? ServiceId { get; set; }
    public Guid? OperatorId { get; set; }
    public string? VoyageType { get; set; }
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
