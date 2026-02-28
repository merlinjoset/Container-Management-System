using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ContainerManagement.Web.Models.Navigation;

public static class NavigationMap
{
    private static readonly IReadOnlyDictionary<MainMenu, string> MainMenuRoutes = new Dictionary<MainMenu, string>
    {
        { MainMenu.Settings, "/settings" },
        { MainMenu.Schedules, "/schedules" },
        { MainMenu.Commercial, "/commercial" },
        { MainMenu.CustomerService, "/customer-service" },
        { MainMenu.Documentation, "/documentation" },
        { MainMenu.Operations, "/operations" },
        { MainMenu.Contract, "/contract" },
        { MainMenu.EMS, "/ems" },
        { MainMenu.EDI, "/edi" }
    };

    private static readonly IReadOnlyDictionary<ModulePage, string> ModuleRoutes = new Dictionary<ModulePage, string>
    {
        { ModulePage.ScheduleViewer, "/schedules/viewer" },
        { ModulePage.Maintenance, "/maintenance" },
        { ModulePage.SlotBuyAndSell, "/commercial/slot-buy-sell" },
        { ModulePage.Vesselinfo, "/vessels/info" },
        { ModulePage.Booking, "/booking" },

        // Settings sub-menu routes
        { ModulePage.Masters, "/settings/masters" },
        { ModulePage.GlobalMasters, "/settings/global-masters" },
        { ModulePage.Operator, "/operators" },
        { ModulePage.Regions, "/regions" },
        { ModulePage.Ports, "/ports" },
        { ModulePage.Terminals, "/terminals" },
        { ModulePage.Vendors, "/vendors" },
        { ModulePage.Restriction, "/settings/restriction" },
        { ModulePage.SystemTools, "/settings/system-tools" },
        { ModulePage.UserAccessSecurity, "/settings/user-access-security" },

        // Operations
        { ModulePage.Vessels, "/vessels" }
    };

    public static string GetMainMenuUrl(MainMenu menu) => MainMenuRoutes.TryGetValue(menu, out var url) ? url : "/";
    public static string GetModuleUrl(ModulePage module) => ModuleRoutes.TryGetValue(module, out var url) ? url : "/";

    public static string GetDisplayName(this Enum value)
    {
        var member = value.GetType().GetMember(value.ToString()).FirstOrDefault();
        var display = member?.GetCustomAttribute<DisplayAttribute>();
        return display?.Name ?? value.ToString();
    }
}
