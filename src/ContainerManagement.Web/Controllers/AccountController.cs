using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContainerManagement.Web.Controllers;

public class AccountController : Controller
{
    private readonly ITokenService _tokenService;

    public AccountController(ITokenService tokenService)
    {
        _tokenService = tokenService;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Login() => View();

    [AllowAnonymous]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(dto);

        try
        {
            var res = await _tokenService.LoginAsync(dto.Email, dto.Password, ct);

            Response.Cookies.Append("access_token", res.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = Request.IsHttps,         
                SameSite = SameSiteMode.Lax,
                Expires = res.ExpiresAtUtc
            });

            return RedirectToAction("Index", "Payments");
        }
        catch (UnauthorizedAccessException)
        {
            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(dto);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return RedirectToAction("Login");
    }
}
