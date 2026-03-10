namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class DistanceEntity
    {
        public Guid Id { get; set; }
        public Guid FromPortId { get; set; }
        public Guid ToPortId { get; set; }
        public decimal Distance { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
