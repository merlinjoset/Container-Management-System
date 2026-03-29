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
        public IActionResult Import()
        {
            return View(new List<VesselImportRowDto>());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Import(IFormFile file, CancellationToken ct)
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select an Excel file.";
                return View(new List<VesselImportRowDto>());
            }
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext != ".xlsx" && ext != ".xls")
            {
                TempData["Error"] = "Unsupported file type. Please upload .xlsx or .xls.";
                return View(new List<VesselImportRowDto>());
            }

            var previewRows = new List<VesselImportRowDto>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                while (reader.Read())
                {
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("vessel") == true) { rowIndex++; continue; }
                    }
                    rowIndex++;

                    string? S(int i) => reader.FieldCount > i ? reader.GetValue(i)?.ToString()?.Trim() : null;

                    var row = new VesselImportRowDto
                    {
                        RowNumber = rowIndex,
                        VesselName = S(0),
                        VesselCode = S(1),
                        ImoCode = S(2),
                        Flag = S(6)
                    };

                    // Validate VesselName required
                    if (string.IsNullOrWhiteSpace(row.VesselName))
                        row.Errors.Add("Vessel Name is required");

                    // Parse Teus (int)
                    var teusStr = S(3);
                    if (!string.IsNullOrWhiteSpace(teusStr))
                    {
                        if (int.TryParse(teusStr, out var teus))
                            row.Teus = teus;
                        else
                            row.Errors.Add("TEUs must be a valid integer");
                    }

                    // Parse NRT (decimal)
                    var nrtStr = S(4);
                    if (!string.IsNullOrWhiteSpace(nrtStr))
                    {
                        if (decimal.TryParse(nrtStr, out var nrt))
                            row.NRT = nrt;
                        else
                            row.Errors.Add("NRT must be a valid number");
                    }

                    // Parse GRT (decimal)
                    var grtStr = S(5);
                    if (!string.IsNullOrWhiteSpace(grtStr))
                    {
                        if (decimal.TryParse(grtStr, out var grt))
                            row.GRT = grt;
                        else
                            row.Errors.Add("GRT must be a valid number");
                    }

                    // Parse Speed (decimal)
                    var speedStr = S(7);
                    if (!string.IsNullOrWhiteSpace(speedStr))
                    {
                        if (decimal.TryParse(speedStr, out var speed))
                            row.Speed = speed;
                        else
                            row.Errors.Add("Speed must be a valid number");
                    }

                    // Parse BuildYear (int)
                    var yearStr = S(8);
                    if (!string.IsNullOrWhiteSpace(yearStr))
                    {
                        if (int.TryParse(yearStr, out var year))
                            row.BuildYear = year;
                        else
                            row.Errors.Add("Build Year must be a valid integer");
                    }

                    previewRows.Add(row);
                }
            }

            ViewBag.ShowPreview = true;
            return View(previewRows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport(List<VesselImportRowDto> rows, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var importRows = rows.Select(r => (
                (string?)r.VesselName, (string?)r.VesselCode, (string?)r.ImoCode,
                (int?)r.Teus, (decimal?)r.NRT, (decimal?)r.GRT,
                (string?)r.Flag, (decimal?)r.Speed, (int?)r.BuildYear
            ));

            var (added, updated, skipped) = await _vesselService.ImportAsync(importRows, userId, ct);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] VesselUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.ModifiedBy = userId;
            await _vesselService.UpdateAsync(dto, ct);
            return Ok(new {
                success = true,
                vesselName = dto.VesselName,
                vesselCode = dto.VesselCode,
                imoCode = dto.ImoCode,
                teus = dto.Teus,
                nrt = dto.NRT,
                grt = dto.GRT,
                flag = dto.Flag,
                speed = dto.Speed,
                buildYear = dto.BuildYear
            });
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _vesselService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Vessels");
            var headers = new[] { "Vessel Name", "Vessel Code", "Imo Code", "TEUs", "NRT", "GRT", "Flag", "Speed", "Build Year" };
            for (int i = 0; i < headers.Length; i++) ws.Cell(1, i + 1).Value = headers[i];
            ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;
            ws.Range(1, 1, 1, headers.Length).Style.Fill.BackgroundColor = XLColor.LightGray;
            var r = 2;
            foreach (var v in list)
            {
                ws.Cell(r,1).Value = v.VesselName;
                ws.Cell(r,2).Value = v.VesselCode;
                ws.Cell(r,3).Value = v.ImoCode;
                ws.Cell(r,4).Value = v.Teus;
                ws.Cell(r,5).Value = v.NRT;
                ws.Cell(r,6).Value = v.GRT;
                ws.Cell(r,7).Value = v.Flag;
                ws.Cell(r,8).Value = v.Speed;
                ws.Cell(r,9).Value = v.BuildYear;
                r++;
            }
            ws.Columns(1, headers.Length).AdjustToContents();
            using var ms = new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Vessels.xlsx");
        }
    }
}
