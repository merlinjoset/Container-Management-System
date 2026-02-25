using ContainerManagement.Application.Dtos.Countries;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExcelDataReader;
using ClosedXML.Excel;

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

        [HttpGet]
        public IActionResult Import()
        {
            return View();
        }

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

            var rows = new List<(string? Name, string? Code)>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                while (reader.Read())
                {
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        var c1 = reader.GetValue(1)?.ToString()?.Trim().ToLowerInvariant();
                        if ((c0?.Contains("country") == true && c0.Contains("name")) || (c1?.Contains("code") == true))
                        { rowIndex++; continue; }
                    }
                    var name = reader.FieldCount > 0 ? reader.GetValue(0)?.ToString() : null;
                    var code = reader.FieldCount > 1 ? reader.GetValue(1)?.ToString() : null;
                    rows.Add((name, code));
                    rowIndex++;
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var (added, updated, skipped) = await _countryService.ImportAsync(rows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Countries");
            ws.Cell(1, 1).Value = "Country Name";
            ws.Cell(1, 2).Value = "Country Code";
            ws.Range(1, 1, 1, 2).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns(1, 2).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "CountriesTemplate.xlsx");
        }
    }
}
