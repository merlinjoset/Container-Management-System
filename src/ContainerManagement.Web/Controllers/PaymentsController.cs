using ContainerManagement.Application.Dtos;
using ContainerManagement.Application.Jobs;
using ContainerManagement.Application.Security;
using ContainerManagement.Application.Services;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers;

public class PaymentsController : Controller
{
    private readonly PaymentService _paymentService;

    public PaymentsController(PaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    // GET /Payments
    [Authorize]
    [HttpGet("/Payments")]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var (userId, isAdmin) = GetUserContext();
        var list = await _paymentService.GetListAsync(userId, isAdmin, ct);

        ViewBag.IsAdmin = isAdmin;
        return View(list); 
    }

    // GET /Payments/Create
    [Authorize]
    [HttpGet("/Payments/Create")]
    public async Task<IActionResult> Create(CancellationToken ct)
    {
        // For dropdown in Create.cshtml
        ViewBag.Providers = await _paymentService.GetProvidersAsync(ct);

        return View(new CreatePaymentRequestDto()); 
    }

    // POST /Payments/Create
    [Authorize]
    [HttpPost("/Payments/Create")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreatePaymentRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
        {
            ViewBag.Providers = await _paymentService.GetProvidersAsync(ct);
            return View(dto);
        }

        var (userId, _) = GetUserContext();

        Task Enqueue(Guid paymentId)
        {
            BackgroundJob.Enqueue<PaymentProcessingJob>(
                "payments",
                job => job.ProcessAsync(paymentId, default)
            );
            return Task.CompletedTask;
        }

        await _paymentService.CreateAsync(dto, userId, Enqueue, ct);
        return RedirectToAction(nameof(Index));
    }

    // POST /api/payments
    [Authorize]
    [HttpPost("/api/payments")]
    public async Task<IActionResult> CreateApi([FromBody] CreatePaymentRequestDto dto, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var (userId, _) = GetUserContext();

        Task Enqueue(Guid paymentId)
        {
            BackgroundJob.Enqueue<PaymentProcessingJob>(
                "payments",
                job => job.ProcessAsync(paymentId, default)
            );
            return Task.CompletedTask;
        }

        var id = await _paymentService.CreateAsync(dto, userId, Enqueue, ct);
        return Ok(new { id });
    }

    // GET /api/payments
    [Authorize]
    [HttpGet("/api/payments")]
    public async Task<IActionResult> GetList(CancellationToken ct)
    {
        var (userId, isAdmin) = GetUserContext();
        var list = await _paymentService.GetListAsync(userId, isAdmin, ct);

        var dto = list.Select(p => new PaymentDto
        {
            Id = p.Id,
            ProviderName = p.Provider?.Name ?? "(unknown)",
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status,
            AttemptCount = p.AttemptCount,
            CreationTimeUtc = p.CreationTimeUtc,
            LastError = p.LastError
        }).ToList();

        return Ok(dto);
    }

    [Authorize(Policy = AppPolicies.PaymentsProcess)] // Admin only
    [HttpPost("/Payments/Retry/{id:guid}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RetryUi(Guid id, CancellationToken ct)
    {
        var (_, isAdmin) = GetUserContext();

        Task Enqueue(Guid paymentId)
        {
            BackgroundJob.Enqueue<PaymentProcessingJob>(
                "payments",
                job => job.ProcessAsync(paymentId, default)
            );
            return Task.CompletedTask;
        }

        await _paymentService.RetryAsync(id, isAdmin, Enqueue, ct);
        return RedirectToAction(nameof(Index));
    }

    private (Guid userId, bool isAdmin) GetUserContext()
    {
        var isAdmin = User.IsInRole(AppRoles.Admin);

        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var userId))
            throw new InvalidOperationException("Authenticated user id is missing/invalid.");

        return (userId, isAdmin);
    }
}
