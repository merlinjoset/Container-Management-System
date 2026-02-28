using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Web.Models.Navigation;

public enum MainMenu
{
    [Display(Name = "Settings")] Settings,
    [Display(Name = "Schedules")] Schedules,
    [Display(Name = "Commercial")] Commercial,
    [Display(Name = "Customer Service")] CustomerService,
    [Display(Name = "Documentation")] Documentation,
    [Display(Name = "Operations")] Operations,
    [Display(Name = "Contract")] Contract,
    [Display(Name = "EMS")] EMS,
    [Display(Name = "EDI")] EDI
}

