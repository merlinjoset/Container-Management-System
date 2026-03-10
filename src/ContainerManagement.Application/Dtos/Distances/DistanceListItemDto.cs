namespace ContainerManagement.Application.Dtos.Distances
{
    public class DistanceListItemDto
    {
        public Guid Id { get; set; }
        public Guid FromPortId { get; set; }
        public Guid ToPortId { get; set; }
        public decimal Distance { get; set; }
        public string? FromPortName { get; set; }
        public string? ToPortName { get; set; }
    }
}
