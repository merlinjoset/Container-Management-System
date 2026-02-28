using ContainerManagement.Web.Models.Navigation;
using Microsoft.AspNetCore.Mvc;

namespace ContainerManagement.Web.Extensions;

public static class UrlNavigationExtensions
{
    // Helper for views/tests to navigate by strongly-typed menu/module
    public static string NavigateToMainMenu(this IUrlHelper url, MainMenu menu)
        => NavigationMap.GetMainMenuUrl(menu);

    public static string OpenModule(this IUrlHelper url, ModulePage module)
        => NavigationMap.GetModuleUrl(module);
}

