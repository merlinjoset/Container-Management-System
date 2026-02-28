using System.ComponentModel.DataAnnotations;

namespace ContainerManagement.Web.Models.Navigation;

public enum ModulePage
{
    [Display(Name = "Schedule Viewer")] ScheduleViewer,
    [Display(Name = "Maintenance")] Maintenance,
    [Display(Name = "Slot Buy & Sell")] SlotBuyAndSell,
    [Display(Name = "Vesselinfo")] Vesselinfo,
    [Display(Name = "Booking")] Booking,

    // Settings sub-menus
    [Display(Name = "Masters")] Masters,
    [Display(Name = "Global Masters")] GlobalMasters,
    [Display(Name = "Operator")] Operator,
    [Display(Name = "Regions")] Regions,
    [Display(Name = "Ports")] Ports,
    [Display(Name = "Terminals")] Terminals,
    [Display(Name = "Vendors")] Vendors,
    [Display(Name = "Restriction")] Restriction,
    [Display(Name = "System Tools")] SystemTools,
    [Display(Name = "User Access Security")] UserAccessSecurity,

    // Operations sub-menus
    [Display(Name = "Vessels")] Vessels
}
