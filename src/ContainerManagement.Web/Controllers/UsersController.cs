using ContainerManagement.Application.Dtos.Auth;
using ContainerManagement.Application.Security;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContainerManagement.Web.Controllers;

[Authorize(Policy = AppPolicies.ProvidersManage)] // Admins only
public class UsersController : Controller
{
    private readonly UserAdminService _svc;
    public UsersController(UserAdminService svc) { _svc = svc; }

    [HttpGet("/Users")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var users = await _svc.GetUsersAsync(ct);
        return View(users);
    }

    [HttpGet("/Users/Create")]
    public IActionResult Create()
        => View(new CreateUserRequestDto());

    [HttpPost("/Users/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateUserRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid) return View(dto);

        try
        {
            await _svc.CreateUserAsync(dto, ct);
            TempData["Msg"] = "User created.";
            return RedirectToAction("Index", "Users");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(nameof(CreateUserRequestDto.Email), ex.Message);
            return View(dto);
        }
    }
}
