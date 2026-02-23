using ContainerManagement.Application.Dtos.Terminals;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers
{
    public class TerminalsController : Controller
    {
        private readonly TerminalService _terminalService;
        private readonly PortService _portService;

        public TerminalsController(TerminalService terminalService, PortService portService)
        {
            _terminalService = terminalService;
            _portService = portService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _terminalService.GetAllAsync(ct);
            return View(list);
        }

        private async Task PopulatePortsAsync(Guid? selectedId, CancellationToken ct)
        {
            var ports = await _portService.GetAllAsync(ct);
            var items = ports
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(p.PortCode) ? p.FullName : $"{p.PortCode} - {p.FullName}",
                    Selected = selectedId.HasValue && p.Id == selectedId.Value
                })
                .ToList();
            ViewBag.Ports = items;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulatePortsAsync(null, ct);
            return View(new TerminalCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TerminalCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePortsAsync(dto.PortId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                await PopulatePortsAsync(dto.PortId, ct);
                return View(dto);
            }
            dto.CreatedBy = userId;

            try
            {
                await _terminalService.CreateAsync(dto, ct);
                TempData["Success"] = "Terminal created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulatePortsAsync(dto.PortId, ct);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var list = await _terminalService.GetAllAsync(ct);
            var t = list.FirstOrDefault(x => x.Id == id);
            if (t == null) return NotFound();

            var dto = new TerminalUpdateDto
            {
                Id = t.Id,
                PortId = t.PortId,
                TerminalName = t.TerminalName,
                TerminalCode = t.TerminalCode
            };

            await PopulatePortsAsync(dto.PortId, ct);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TerminalUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePortsAsync(dto.PortId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _terminalService.UpdateAsync(dto, ct);
                TempData["Success"] = "Terminal updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulatePortsAsync(dto.PortId, ct);
                return View(dto);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            await _terminalService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Terminal deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

