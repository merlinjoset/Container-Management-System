namespace ContainerManagement.Application.Dtos.Distances
{
    public class DistanceUpdateDto
    {
        public Guid Id { get; set; }
        public Guid FromPortId { get; set; }
        public Guid ToPortId { get; set; }
        public decimal Distance { get; set; }
        public Guid ModifiedBy { get; set; }
    }
}
