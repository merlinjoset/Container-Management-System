using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Web.Models;

public class ProviderEditVm
{
    public Guid Id { get; set; }

    [Required, StringLength(128)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(512)]
    [Display(Name = "Endpoint URL")]
    public string EndpointUrl { get; set; } = string.Empty;

    [Display(Name = "Active")]
    public bool IsActive { get; set; }
}
