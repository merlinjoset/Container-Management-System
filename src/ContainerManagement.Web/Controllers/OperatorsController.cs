using ContainerManagement.Application.Dtos.Operators;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers
{
    public class OperatorsController : Controller
    {
        private readonly OperatorService _operatorService;
        private readonly VendorService _vendorService;
        private readonly CountryService _countryService;

        public OperatorsController(OperatorService operatorService, VendorService vendorService, CountryService countryService)
        {
            _operatorService = operatorService;
            _vendorService = vendorService;
            _countryService = countryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _operatorService.GetAllAsync(ct);
            return View(list);
        }

        private async Task PopulateLookupsAsync(Guid? vendorId, Guid? countryId, CancellationToken ct)
        {
            var vendors = await _vendorService.GetAllAsync(ct);
            var vendorItems = vendors.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.VendorCode} - {v.VendorName}",
                Selected = vendorId.HasValue && v.Id == vendorId.Value
            }).ToList();

            var countries = await _countryService.GetAllAsync(ct);
            var countryItems = countries.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(c.CountryCode) ? c.Country : $"{c.CountryCode} - {c.Country}",
                Selected = countryId.HasValue && c.Id == countryId.Value
            }).ToList();

            ViewBag.Vendors = vendorItems;
            ViewBag.Countries = countryItems;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateLookupsAsync(null, null, ct);
            return View(new OperatorCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OperatorCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
                return View(dto);
            }
            dto.CreatedBy = userId;

            try
            {
                await _operatorService.CreateAsync(dto, ct);
                TempData["Success"] = "Operator created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var list = await _operatorService.GetAllAsync(ct);
            var op = list.FirstOrDefault(x => x.Id == id);
            if (op == null) return NotFound();

            var dto = new OperatorUpdateDto
            {
                Id = op.Id,
                OperatorName = op.OperatorName,
                UniqueCode = op.UniqueCode,
                VendorId = op.VendorId,
                CountryId = op.CountryId
            };

            await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OperatorUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _operatorService.UpdateAsync(dto, ct);
                TempData["Success"] = "Operator updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
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

            await _operatorService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Operator deleted successfully.";
            return RedirectToAction(nameof(Index));
        }
    }
}

