using ContainerManagement.Domain.Common;

namespace ContainerManagement.Domain.Voyages;

public class EditRequest : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid VoyagePortId { get; set; }
    /// <summary>
    /// Arrival, Departure, or TOS
    /// </summary>
    public string ReportType { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    /// <summary>
    /// Pending, Approved, Rejected
    /// </summary>
    public string Status { get; set; } = "Pending";
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedOn { get; set; }
}
