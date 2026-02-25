using ContainerManagement.Application.Dtos.Vessels;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExcelDataReader;
using ClosedXML.Excel;

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

            var rows = new List<(string? Name, string? Code, string? Imo, int? Teus, decimal? Nrt, decimal? Grt, string? Flag, decimal? Speed, int? Year)>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                while (reader.Read())
                {
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("vessel") == true && c0.Contains("code")) { rowIndex++; continue; }
                    }
                    string? S(int i) => reader.FieldCount > i ? reader.GetValue(i)?.ToString() : null;
                    int? Int(int i) { if (reader.FieldCount <= i) return null; return int.TryParse(reader.GetValue(i)?.ToString(), out var v) ? v : null; }
                    decimal? Dec(int i) { if (reader.FieldCount <= i) return null; return decimal.TryParse(reader.GetValue(i)?.ToString(), out var v) ? v : null; }
                    rows.Add((S(0), S(1), S(2), Int(3), Dec(4), Dec(5), S(6), Dec(7), Int(8)));
                    rowIndex++;
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var (added, updated, skipped) = await _vesselService.ImportAsync(rows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Vessels");
            var headers = new[] { "Vessel Name", "Vessel Code", "Imo Code", "Teus", "NRT", "GRT", "Flag", "Speed", "Build Year" };
            for (int i = 0; i < headers.Length; i++)
                ws.Cell(1, i + 1).Value = headers[i];
            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;
            ws.Range(1, 1, 1, headers.Length).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns(1, headers.Length).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "VesselsTemplate.xlsx");
        }
    }
}
