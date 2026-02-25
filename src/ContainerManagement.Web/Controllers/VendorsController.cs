using ContainerManagement.Application.Dtos.Vendors;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using ExcelDataReader;
using ClosedXML.Excel;

namespace ContainerManagement.Web.Controllers
{
    public class VendorsController : Controller
    {
        private readonly VendorService _vendorService;
        private readonly CountryService _countryService;

        public VendorsController(VendorService vendorService, CountryService countryService)
        {
            _vendorService = vendorService;
            _countryService = countryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var vendors = await _vendorService.GetAllAsync(ct);
            return View(vendors);
        }

        private async Task PopulateCountriesAsync(Guid? selectedId, CancellationToken ct)
        {
            var countries = await _countryService.GetAllAsync(ct);
            var items = countries
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(c.CountryCode) ? c.CountryName : $"{c.CountryCode} - {c.CountryName}",
                    Selected = selectedId.HasValue && c.Id == selectedId.Value
                })
                .ToList();
            ViewBag.Countries = items;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateCountriesAsync(null, ct);
            return View(new VendorCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(VendorCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCountriesAsync(dto.CountryId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                await PopulateCountriesAsync(dto.CountryId, ct);
                return View(dto);
            }
            dto.CreatedBy = userId;

            try
            {
                await _vendorService.CreateAsync(dto, ct);
                TempData["Success"] = "Vendor created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateCountriesAsync(dto.CountryId, ct);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var list = await _vendorService.GetAllAsync(ct);
            var v = list.FirstOrDefault(x => x.Id == id);
            if (v == null) return NotFound();

            var dto = new VendorUpdateDto
            {
                Id = v.Id,
                VendorName = v.VendorName,
                VendorCode = v.VendorCode,
                CountryId = v.CountryId
            };

            await PopulateCountriesAsync(dto.CountryId, ct);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(VendorUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateCountriesAsync(dto.CountryId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _vendorService.UpdateAsync(dto, ct);
                TempData["Success"] = "Vendor updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulateCountriesAsync(dto.CountryId, ct);
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

            await _vendorService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Vendor deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Import() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select an Excel file.";
                return RedirectToAction(nameof(Import));
            }
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                TempData["Error"] = "Unsupported file type. Please upload .xlsx or .xls.";
                return RedirectToAction(nameof(Import));
            }

            var rows = new List<(string? VendorName, string? VendorCode, string? CountryCode)>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                while (reader.Read())
                {
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("vendor") == true && c0.Contains("code"))
                        { rowIndex++; continue; }
                    }
                    string? S(int i) => reader.FieldCount > i ? reader.GetValue(i)?.ToString() : null;
                    rows.Add((S(0), S(1), S(2)));
                    rowIndex++;
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            var (added, updated, skipped) = await _vendorService.ImportAsync(rows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Vendors");
            ws.Cell(1, 1).Value = "Vendor Name";
            ws.Cell(1, 2).Value = "Vendor Code";
            ws.Cell(1, 3).Value = "Country Code";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns(1, 3).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "VendorsTemplate.xlsx");
        }
    }
}
