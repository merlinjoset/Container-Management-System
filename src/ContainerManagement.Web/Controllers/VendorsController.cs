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
            await PopulateCountriesAsync(null, ct);
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
        public async Task<IActionResult> Import(CancellationToken ct)
        {
            await PopulateCountriesAsync(null, ct);
            return View(new List<VendorImportRowDto>());
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

            var previewRows = new List<VendorImportRowDto>();
            int maxNonEmptyCol = -1;
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                var rowNum = 1;
                while (reader.Read())
                {
                    // Track max non-empty column across all rows (header + data)
                    for (int c = 0; c < reader.FieldCount; c++)
                    {
                        var v = reader.GetValue(c)?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(v) && c > maxNonEmptyCol)
                            maxNonEmptyCol = c;
                    }

                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("vendor") == true)
                        { rowIndex++; continue; }
                    }
                    var name = reader.FieldCount > 0 ? reader.GetValue(0)?.ToString()?.Trim() : null;
                    var code = reader.FieldCount > 1 ? reader.GetValue(1)?.ToString()?.Trim() : null;
                    var ccode = reader.FieldCount > 2 ? reader.GetValue(2)?.ToString()?.Trim() : null;

                    if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(code))
                    {
                        previewRows.Add(new VendorImportRowDto
                        {
                            RowNumber = rowNum++,
                            VendorName = name,
                            VendorCode = code,
                            CountryCode = ccode
                        });
                    }
                    rowIndex++;
                }
            }

            // Reject the file if any row has data in more than 3 columns
            if (maxNonEmptyCol > 2)
            {
                TempData["Error"] = $"The file has {maxNonEmptyCol + 1} columns with data. Vendor import accepts exactly 3 columns: Vendor Name (A), Vendor Code (B), Country Code (C). Please remove extra columns and try again.";
                return RedirectToAction(nameof(Import));
            }

            var countries = await _countryService.GetAllAsync(ct);

            foreach (var row in previewRows)
            {
                if (string.IsNullOrWhiteSpace(row.VendorName))
                    row.Errors.Add("Vendor Name is required.");

                if (string.IsNullOrWhiteSpace(row.CountryCode))
                {
                    row.Errors.Add("Country Code is required.");
                }
                else
                {
                    var match = countries.FirstOrDefault(c =>
                        string.Equals(c.CountryCode, row.CountryCode, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                        row.CountryId = match.Id;
                    else
                        row.Errors.Add($"Country Code '{row.CountryCode}' not found in database.");
                }
            }

            var errorCount = previewRows.Count(r => r.HasErrors);
            if (errorCount > 0)
                ViewBag.ErrorSummary = $"{errorCount} row(s) have validation errors. Please correct them before importing.";

            await PopulateCountriesAsync(null, ct);
            ViewBag.ShowPreview = true;
            return View(previewRows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport(List<VendorImportRowDto> rows, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var countries = await _countryService.GetAllAsync(ct);

            var importRows = new List<(string? VendorName, string? VendorCode, string? CountryCode)>();
            foreach (var row in rows)
            {
                var countryCode = countries.FirstOrDefault(c => c.Id == row.CountryId)?.CountryCode;
                importRows.Add((row.VendorName, row.VendorCode, countryCode));
            }

            var (added, updated, skipped) = await _vendorService.ImportAsync(importRows, userId, ct);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineCreate([FromBody] VendorCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.CreatedBy = userId;
            var id = await _vendorService.CreateAsync(dto, ct);
            var countries = await _countryService.GetAllAsync(ct);
            var countryName = countries.FirstOrDefault(c => c.Id == dto.CountryId)?.CountryName;
            return Ok(new { success = true, id, vendorName = dto.VendorName, vendorCode = dto.VendorCode, countryName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] VendorUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.ModifiedBy = userId;
            await _vendorService.UpdateAsync(dto, ct);
            var countries = await _countryService.GetAllAsync(ct);
            var countryName = countries.FirstOrDefault(c=>c.Id==dto.CountryId)?.CountryName;
            return Ok(new { success=true, vendorName=dto.VendorName, vendorCode=dto.VendorCode, countryName });
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _vendorService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Vendors");
            ws.Cell(1,1).Value = "Vendor Name";
            ws.Cell(1,2).Value = "Vendor Code";
            ws.Cell(1,3).Value = "Country";
            ws.Range(1,1,1,3).Style.Font.Bold = true;
            ws.Range(1,1,1,3).Style.Fill.BackgroundColor = XLColor.LightGray;
            var r=2; foreach (var v in list){ ws.Cell(r,1).Value=v.VendorName; ws.Cell(r,2).Value=v.VendorCode; ws.Cell(r,3).Value=v.CountryName; r++; }
            ws.Columns(1,3).AdjustToContents();
            using var ms = new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Vendors.xlsx");
        }
    }
}
