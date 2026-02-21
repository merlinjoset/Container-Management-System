using ContainerManagement.Application.Dtos.Ports;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers
{
    public class PortsController : Controller
    {
        private readonly PortService _portService;

        public PortsController(PortService portService)
        {
            _portService = portService;
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var ports = await _portService.GetAllAsync(ct);
            return View(ports);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PortCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(dto);

            // DB requires CreatedBy/ModifiedBy (NOT NULL)
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                return View(dto);
            }

            dto.CreatedBy = userId;

            try
            {
                await _portService.CreateAsync(dto, ct);
                TempData["Success"] = "Port created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            // If you later add GetByIdAsync in service, replace this with that call.
            var ports = await _portService.GetAllAsync(ct);
            var port = ports.FirstOrDefault(x => x.Id == id);

            if (port == null)
                return NotFound();

            var dto = new PortUpdateDto
            {
                Id = port.Id,
                PortCode = port.PortCode,
                FullName = port.FullName,
                Country = port.Country,
                Region = port.Region,
                RegionCode = port.RegionCode
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PortUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _portService.UpdateAsync(dto, ct);
                TempData["Success"] = "Port updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
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

            await _portService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Port deleted successfully.";
            return RedirectToAction(nameof(Index));
        }


    }
}
