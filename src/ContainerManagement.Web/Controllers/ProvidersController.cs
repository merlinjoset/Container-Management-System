using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Security;
using ContainerManagement.Domain.Providers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ContainerManagement.Web.Models;

namespace ContainerManagement.Web.Controllers;

[Authorize(Policy = AppPolicies.ProvidersManage)]
public class ProvidersController : Controller
{
    private readonly IProviderRepository _providers;

    public ProvidersController(IProviderRepository providers)
    {
        _providers = providers;
    }

    // GET: /Providers
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var list = await _providers.GetListAsync(ct);
        return View(list); 
    }

    // GET: /Providers/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new ProviderEditVm { IsActive = true });
    }

    // POST: /Providers/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProviderEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var p = new PaymentProvider
        {
            Id = Guid.NewGuid(),
            Name = vm.Name.Trim(),
            EndpointUrl = vm.EndpointUrl.Trim(),
            IsActive = vm.IsActive
        };

        await _providers.AddAsync(p, ct);
        await _providers.SaveChangesAsync(ct);

        return RedirectToAction(nameof(Index));
    }

    // GET: /Providers/Edit/{id}
    [HttpGet]
    public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
    {
        var p = await _providers.GetAsync(id, ct);
        if (p == null) return NotFound();

        var vm = new ProviderEditVm
        {
            Id = p.Id,
            Name = p.Name,
            EndpointUrl = p.EndpointUrl,
            IsActive = p.IsActive
        };

        return View(vm); 
    }

    // POST: /Providers/Edit
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ProviderEditVm vm, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return View(vm);

        var p = new PaymentProvider
        {
            Id = vm.Id,
            Name = vm.Name.Trim(),
            EndpointUrl = vm.EndpointUrl.Trim(),
            IsActive = vm.IsActive
        };

        await _providers.UpdateAsync(p, ct);
        await _providers.SaveChangesAsync(ct);

        return RedirectToAction(nameof(Index));
    }

    // POST: /Providers/Delete
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _providers.DeleteAsync(id, ct);
        await _providers.SaveChangesAsync(ct);
        return RedirectToAction(nameof(Index));
    }
}
