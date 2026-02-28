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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select an Excel file (.xlsx or .xls).";
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
                    // Attempt to skip header if present
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        var c1 = reader.GetValue(1)?.ToString()?.Trim().ToLowerInvariant();
                        if ((c0?.Contains("region") == true && c0.Contains("name")) || (c1?.Contains("code") == true))
                        {
                            rowIndex++;
                            continue;
                        }
                    }

                    var name = reader.FieldCount > 0 ? reader.GetValue(0)?.ToString() : null;
                    var code = reader.FieldCount > 1 ? reader.GetValue(1)?.ToString() : null;
                    rows.Add((name, code));
                    rowIndex++;
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var (added, updated, skipped) = await _regionService.ImportAsync(rows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Regions");
            // Headers
            ws.Cell(1, 1).Value = "Region Name";
            ws.Cell(1, 2).Value = "Region Code";
            ws.Range(1, 1, 1, 2).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.Columns(1, 2).AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var bytes = ms.ToArray();
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, "RegionsTemplate.xlsx");
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
