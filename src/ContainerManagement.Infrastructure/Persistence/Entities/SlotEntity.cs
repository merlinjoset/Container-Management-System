namespace ContainerManagement.Infrastructure.Persistence.Entities
{
    public class SlotEntity
    {
        public Guid Id { get; set; }
        public string? SlotName { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public Guid CreatedBy { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
