using ContainerManagement.Application.Dtos.Countries;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers
{
    public class CountriesController : Controller
    {
        private readonly CountryService _countryService;

        public CountriesController(CountryService countryService)
        {
            _countryService = countryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var countries = await _countryService.GetAllAsync(ct);
            return View(countries);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new CountryCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CountryCreateDto dto, CancellationToken ct)
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
                await _countryService.CreateAsync(dto, ct);
                TempData["Success"] = "Country created successfully.";
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
            var countries = await _countryService.GetAllAsync(ct);
            var country = countries.FirstOrDefault(x => x.Id == id);
            if (country == null)
                return NotFound();

            var dto = new CountryUpdateDto
            {
                Id = country.Id,
                CountryName = country.CountryName,
                CountryCode = country.CountryCode
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CountryUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _countryService.UpdateAsync(dto, ct);
                TempData["Success"] = "Country updated successfully.";
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

            await _countryService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Country deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

