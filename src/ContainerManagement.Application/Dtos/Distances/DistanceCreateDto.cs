namespace ContainerManagement.Application.Dtos.Distances
{
    public class DistanceCreateDto
    {
        public Guid FromPortId { get; set; }
        public Guid ToPortId { get; set; }
        public decimal Distance { get; set; }
        public Guid CreatedBy { get; set; }
    }
}
