namespace ContainerManagement.Application.Dtos.Operators
{
    public class OperatorListItemDto
    {
        public Guid Id { get; set; }
        public string OperatorName { get; set; }
        public Guid VendorId { get; set; }
        public string? VendorName { get; set; }
        public bool IsCompetitor { get; set; }
    }
}
