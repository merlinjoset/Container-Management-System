namespace ContainerManagement.Infrastructure.Persistence.Entities;

public class ShipProductivityEntity
{
    public Guid Id { get; set; }
    public Guid TosId { get; set; }
    public decimal? PortStayTime { get; set; }
    public decimal? ProductivityPerHr { get; set; }
    public decimal? MovesPerCrane { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedOn { get; set; }
    public DateTime ModifiedOn { get; set; }
    public Guid CreatedBy { get; set; }
    public Guid ModifiedBy { get; set; }
}
