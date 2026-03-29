using ContainerManagement.Application.Dtos.Ports;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using ExcelDataReader;
using ClosedXML.Excel;

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

        private async Task PopulateLookupsAsync(Guid? selectedCountryId, Guid? selectedRegionId, CancellationToken ct)
        {
            var countries = await _countryService.GetAllAsync(ct);
            ViewBag.Countries = countries
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(c.CountryCode) ? c.CountryName : $"{c.CountryCode} - {c.CountryName}",
                    Selected = selectedCountryId.HasValue && c.Id == selectedCountryId.Value
                })
                .ToList();

            var regions = await _regionService.GetAllAsync(ct);
            ViewBag.Regions = regions
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(r.RegionCode) ? r.RegionName : $"{r.RegionCode} - {r.RegionName}",
                    Selected = selectedRegionId.HasValue && r.Id == selectedRegionId.Value
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
            await PopulateLookupsAsync(null, null, ct);
            return View(ports);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(PortCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(dto.CountryId, dto.RegionId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                await PopulateLookupsAsync(dto.CountryId, dto.RegionId, ct);
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
                await PopulateLookupsAsync(dto.CountryId, dto.RegionId, ct);
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
                CountryId = port.CountryId,
                RegionId = port.RegionId
            };

            await PopulateLookupsAsync(dto.CountryId, dto.RegionId, ct);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(PortUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(dto.CountryId, dto.RegionId, ct);
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
                await PopulateLookupsAsync(dto.CountryId, dto.RegionId, ct);
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

        [HttpGet]
        public async Task<IActionResult> Import(CancellationToken ct)
        {
            await PopulateLookupsAsync(null, null, ct);
            return View(new List<PortImportRowDto>());
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

            // Parse Excel rows
            var previewRows = new List<PortImportRowDto>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                var rowNum = 1;
                while (reader.Read())
                {
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("port") == true && c0.Contains("code"))
                        { rowIndex++; continue; }
                    }
                    var pcode = reader.FieldCount > 0 ? reader.GetValue(0)?.ToString()?.Trim() : null;
                    var name = reader.FieldCount > 1 ? reader.GetValue(1)?.ToString()?.Trim() : null;
                    var ccode = reader.FieldCount > 2 ? reader.GetValue(2)?.ToString()?.Trim() : null;
                    var rcode = reader.FieldCount > 3 ? reader.GetValue(3)?.ToString()?.Trim() : null;

                    if (!string.IsNullOrWhiteSpace(pcode))
                    {
                        previewRows.Add(new PortImportRowDto
                        {
                            RowNumber = rowNum++,
                            PortCode = pcode,
                            FullName = name,
                            CountryCode = ccode,
                            RegionCode = rcode
                        });
                    }
                    rowIndex++;
                }
            }

            // Match country/region codes to IDs from DB
            var countries = await _countryService.GetAllAsync(ct);
            var regions = await _regionService.GetAllAsync(ct);

            foreach (var row in previewRows)
            {
                // Validate Port Code
                if (string.IsNullOrWhiteSpace(row.PortCode))
                    row.Errors.Add("Port Code is required.");

                // Validate & match Country Code
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

                // Validate & match Region Code
                if (string.IsNullOrWhiteSpace(row.RegionCode))
                {
                    row.Errors.Add("Region Code is required.");
                }
                else
                {
                    var match = regions.FirstOrDefault(r =>
                        string.Equals(r.RegionCode, row.RegionCode, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                        row.RegionId = match.Id;
                    else
                        row.Errors.Add($"Region Code '{row.RegionCode}' not found in database.");
                }
            }

            var errorCount = previewRows.Count(r => r.HasErrors);
            if (errorCount > 0)
                ViewBag.ErrorSummary = $"{errorCount} row(s) have validation errors. Please correct them before importing.";

            await PopulateLookupsAsync(null, null, ct);
            ViewBag.ShowPreview = true;
            return View(previewRows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport(List<PortImportRowDto> rows, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            // Convert preview rows to import tuples — use CountryId/RegionId directly
            var countries = await _countryService.GetAllAsync(ct);
            var regions = await _regionService.GetAllAsync(ct);

            var importRows = new List<(string? PortCode, string? FullName, string? CountryCode, string? RegionCode)>();
            foreach (var row in rows)
            {
                // Resolve the selected IDs back to codes for the existing ImportAsync method
                var countryCode = countries.FirstOrDefault(c => c.Id == row.CountryId)?.CountryCode;
                var regionCode = regions.FirstOrDefault(r => r.Id == row.RegionId)?.RegionCode;

                importRows.Add((row.PortCode, row.FullName, countryCode, regionCode));
            }

            var (added, updated, skipped) = await _portService.ImportAsync(importRows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Ports");
            ws.Cell(1, 1).Value = "Port Code";
            ws.Cell(1, 2).Value = "Full Name";
            ws.Cell(1, 3).Value = "Country Code";
            ws.Cell(1, 4).Value = "Region Code";
            ws.Range(1, 1, 1, 4).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 4).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns(1, 4).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "PortsTemplate.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineCreate([FromBody] PortCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.CreatedBy = userId;
            var id = await _portService.CreateAsync(dto, ct);
            var countries = await _countryService.GetAllAsync(ct);
            var regions = await _regionService.GetAllAsync(ct);
            var countryName = countries.FirstOrDefault(c => c.Id == dto.CountryId)?.CountryName;
            var regionName = regions.FirstOrDefault(r => r.Id == dto.RegionId)?.RegionName;
            return Ok(new { success = true, id, portCode = dto.PortCode, fullName = dto.FullName, countryName, regionName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] PortUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.ModifiedBy = userId;
            await _portService.UpdateAsync(dto, ct);
            var countries = await _countryService.GetAllAsync(ct);
            var regions = await _regionService.GetAllAsync(ct);
            var countryName = countries.FirstOrDefault(c=>c.Id==dto.CountryId)?.CountryName;
            var regionName = regions.FirstOrDefault(r=>r.Id==dto.RegionId)?.RegionName;
            return Ok(new { success=true, fullName=dto.FullName, portCode=dto.PortCode, countryName, regionName });
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _portService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Ports");
            ws.Cell(1,1).Value = "Port Code";
            ws.Cell(1,2).Value = "Full Name";
            ws.Cell(1,3).Value = "Country";
            ws.Cell(1,4).Value = "Region";
            ws.Range(1,1,1,4).Style.Font.Bold = true;
            ws.Range(1,1,1,4).Style.Fill.BackgroundColor = XLColor.LightGray;
            var r=2; foreach (var it in list){ ws.Cell(r,1).Value=it.PortCode; ws.Cell(r,2).Value=it.FullName; ws.Cell(r,3).Value=it.CountryName; ws.Cell(r,4).Value=it.RegionName; r++; }
            ws.Columns(1,4).AdjustToContents();
            using var ms=new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Ports.xlsx");
        }
    }
}
