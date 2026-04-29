using ContainerManagement.Application.Dtos.Regions;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExcelDataReader;
using ClosedXML.Excel;

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

        [HttpGet]
        public IActionResult Import()
        {
            return View(new List<RegionImportRowDto>());
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

            // Read entire sheet into a 2D string grid
            var grid = new List<string?[]>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                while (reader.Read())
                {
                    var fc = reader.FieldCount;
                    var row = new string?[fc];
                    for (int c = 0; c < fc; c++)
                    {
                        row[c] = reader.GetValue(c)?.ToString()?.Trim();
                    }
                    grid.Add(row);
                }
            }

            // Reject the file if any row has data in more than 2 columns
            // (Region template has exactly 2 columns: Region Name, Region Code)
            int maxNonEmptyCol = -1;
            foreach (var row in grid)
            {
                for (int c = 0; c < row.Length; c++)
                {
                    if (!string.IsNullOrWhiteSpace(row[c]) && c > maxNonEmptyCol)
                        maxNonEmptyCol = c;
                }
            }
            if (maxNonEmptyCol > 1)
            {
                TempData["Error"] = $"The file has {maxNonEmptyCol + 1} columns with data. Region import accepts exactly 2 columns: Region Name (A) and Region Code (B). Please remove extra columns and try again.";
                return RedirectToAction(nameof(Import));
            }

            // Detect header row + column positions for Name and Code (auto-detect by keyword)
            int nameCol = 0, codeCol = 1, dataStart = 0;
            if (grid.Count > 0)
            {
                var h = grid[0];
                bool looksLikeHeader = false;
                int? detectedNameCol = null, detectedCodeCol = null;
                for (int c = 0; c < h.Length; c++)
                {
                    var v = (h[c] ?? string.Empty).ToLowerInvariant();
                    if (v.Contains("name")) { detectedNameCol = c; looksLikeHeader = true; }
                    if (v.Contains("code")) { detectedCodeCol = c; looksLikeHeader = true; }
                    if (v.Contains("region") && !v.Contains("code")) { detectedNameCol = c; looksLikeHeader = true; }
                }
                if (looksLikeHeader)
                {
                    nameCol = detectedNameCol ?? 0;
                    codeCol = detectedCodeCol ?? 1;
                    if (nameCol == codeCol) codeCol = (nameCol == 0) ? 1 : 0;
                    dataStart = 1;
                }
            }

            var previewRows = new List<RegionImportRowDto>();
            var rowNum = 1;
            for (int r = dataStart; r < grid.Count; r++)
            {
                var row = grid[r];
                var name = (nameCol < row.Length ? row[nameCol] : null);
                var code = (codeCol < row.Length ? row[codeCol] : null);
                if (string.IsNullOrWhiteSpace(name) && string.IsNullOrWhiteSpace(code)) continue;

                var item = new RegionImportRowDto
                {
                    RowNumber = rowNum++,
                    RegionName = name,
                    RegionCode = code
                };
                if (string.IsNullOrWhiteSpace(name)) item.Errors.Add("Region Name is required.");
                if (string.IsNullOrWhiteSpace(code)) item.Errors.Add("Region Code is required.");
                previewRows.Add(item);
            }

            if (previewRows.Count == 0)
            {
                ViewBag.ErrorSummary = "No data rows found. Please ensure your file has 'Region Name' and 'Region Code' columns with at least one data row below the header.";
            }
            else
            {
                var errorCount = previewRows.Count(r => r.HasErrors);
                if (errorCount > 0)
                    ViewBag.ErrorSummary = $"{errorCount} row(s) have validation errors.";
            }

            ViewBag.ShowPreview = true;
            return View(previewRows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport(List<RegionImportRowDto> rows, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var importRows = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.RegionName))
                .Select(r => ((string?)r.RegionName, (string?)r.RegionCode));

            var (added, updated, skipped) = await _regionService.ImportAsync(importRows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Regions");

            // Headers (row 1) — bold + gray fill
            ws.Cell(1, 1).Value = "Region Name";
            ws.Cell(1, 2).Value = "Region Code";
            ws.Range(1, 1, 1, 2).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

            // Sample rows (in italic gray) so users see the expected format — delete and replace with real data
            ws.Cell(2, 1).Value = "Asia"; ws.Cell(2, 2).Value = "AS";
            ws.Cell(3, 1).Value = "Europe"; ws.Cell(3, 2).Value = "EU";
            ws.Cell(4, 1).Value = "North America"; ws.Cell(4, 2).Value = "NA";
            ws.Range(2, 1, 4, 2).Style.Font.Italic = true;
            ws.Range(2, 1, 4, 2).Style.Font.FontColor = XLColor.DarkGray;

            ws.Column(1).Width = 30;
            ws.Column(2).Width = 18;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var bytes = ms.ToArray();
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, "RegionsTemplate.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineCreate([FromBody] RegionCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.CreatedBy = userId;
            var id = await _regionService.CreateAsync(dto, ct);
            return Ok(new { success = true, id, regionName = dto.RegionName, regionCode = dto.RegionCode });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] RegionUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.ModifiedBy = userId;
            await _regionService.UpdateAsync(dto, ct);
            return Ok(new { success = true, regionName = dto.RegionName, regionCode = dto.RegionCode });
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _regionService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Regions");
            ws.Cell(1,1).Value = "Region Name";
            ws.Cell(1,2).Value = "Region Code";
            ws.Range(1,1,1,2).Style.Font.Bold = true;
            ws.Range(1,1,1,2).Style.Fill.BackgroundColor = XLColor.LightGray;
            var r = 2;
            foreach (var item in list){ ws.Cell(r,1).Value = item.RegionName; ws.Cell(r,2).Value = item.RegionCode; r++; }
            ws.Columns(1,2).AdjustToContents();
            using var ms = new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Regions.xlsx");
        }
    }
}
