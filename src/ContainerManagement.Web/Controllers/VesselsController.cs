using ContainerManagement.Application.Dtos.Vessels;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers
{
    public class VesselsController : Controller
    {
        private readonly VesselService _vesselService;

        public VesselsController(VesselService vesselService)
        {
            _vesselService = vesselService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var vessels = await _vesselService.GetAllAsync(ct);
            return View(vessels);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new VesselCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VesselCreateDto dto, CancellationToken ct)
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
                await _vesselService.CreateAsync(dto, ct);
                TempData["Success"] = "Vessel created successfully.";
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
            var list = await _vesselService.GetAllAsync(ct);
            var v = list.FirstOrDefault(x => x.Id == id);
            if (v == null) return NotFound();

            var dto = new VesselUpdateDto
            {
                Id = v.Id,
                VesselName = v.VesselName,
                VesselCode = v.VesselCode,
                ImoCode = v.ImoCode,
                Teus = v.Teus,
                NRT = v.NRT,
                GRT = v.GRT,
                Flag = v.Flag,
                Speed = v.Speed,
                BuildYear = v.BuildYear
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VesselUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _vesselService.UpdateAsync(dto, ct);
                TempData["Success"] = "Vessel updated successfully.";
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

            await _vesselService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Vessel deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

