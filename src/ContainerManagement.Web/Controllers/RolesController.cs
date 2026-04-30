using ContainerManagement.Application.Security;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContainerManagement.Web.Controllers;

[Authorize(Policy = AppPolicies.ProvidersManage)] // Admins only — same policy as Users
[Route("Roles")]
public class RolesController : Controller
{
    private readonly UserAdminService _svc;

    public RolesController(UserAdminService svc)
    {
        _svc = svc;
    }

    [HttpGet("")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var roles = await _svc.GetRolesAsync(ct);
        return View(roles);
    }
}
