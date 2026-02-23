using ContainerManagement.Application.Dtos.Regions;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers
{
    public class RegionsController : Controller
    {
        private readonly RegionService _regionService;

        public RegionsController(RegionService regionService)
        {
            _regionService = regionService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var regions = await _regionService.GetAllAsync(ct);
            return View(regions);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new RegionCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RegionCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                return View(dto);
            }
            dto.CreatedBy = userId;

            try
            {
                await _regionService.CreateAsync(dto, ct);
                TempData["Success"] = "Region created successfully.";
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
            var regions = await _regionService.GetAllAsync(ct);
            var region = regions.FirstOrDefault(x => x.Id == id);
            if (region == null)
                return NotFound();

            var dto = new RegionUpdateDto
            {
                Id = region.Id,
                RegionName = region.RegionName,
                RegionCode = region.RegionCode
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RegionUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _regionService.UpdateAsync(dto, ct);
                TempData["Success"] = "Region updated successfully.";
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

            await _regionService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Region deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

