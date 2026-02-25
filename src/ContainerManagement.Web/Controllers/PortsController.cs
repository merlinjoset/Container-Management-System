using ContainerManagement.Application.Dtos.Ports;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers
{
    public class PortsController : Controller
    {
        private readonly PortService _portService;
        private readonly CountryService _countryService;
        private readonly RegionService _regionService;

        public PortsController(PortService portService, CountryService countryService, RegionService regionService)
        {
            _portService = portService;
            _countryService = countryService;
            _regionService = regionService;
        }

        private async Task PopulateLookupsAsync(string? selectedCountry, string? selectedRegion, CancellationToken ct)
        {
            var countries = await _countryService.GetAllAsync(ct);
            ViewBag.Countries = countries
                .Select(c => new SelectListItem
                {
                    Value = c.CountryName ?? string.Empty,
                    Text = string.IsNullOrWhiteSpace(c.CountryCode) ? c.CountryName : $"{c.CountryCode} - {c.CountryName}",
                    Selected = !string.IsNullOrWhiteSpace(selectedCountry) && string.Equals(c.CountryName, selectedCountry, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();

            var regions = await _regionService.GetAllAsync(ct);
            ViewBag.Regions = regions
                .Select(r => new SelectListItem
                {
                    Value = r.RegionName ?? string.Empty,
                    Text = string.IsNullOrWhiteSpace(r.RegionCode) ? r.RegionName : $"{r.RegionCode} - {r.RegionName}",
                    Selected = !string.IsNullOrWhiteSpace(selectedRegion) && string.Equals(r.RegionName, selectedRegion, StringComparison.OrdinalIgnoreCase)
                })
                .ToList();
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateLookupsAsync(null, null, ct);
            return View(new PortCreateDto());
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
            {
                await PopulateLookupsAsync(dto.Country, dto.Region, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                await PopulateLookupsAsync(dto.Country, dto.Region, ct);
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
                await PopulateLookupsAsync(dto.Country, dto.Region, ct);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
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

            await PopulateLookupsAsync(dto.Country, dto.Region, ct);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PortUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(dto.Country, dto.Region, ct);
                return View(dto);
            }

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
                await PopulateLookupsAsync(dto.Country, dto.Region, ct);
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

