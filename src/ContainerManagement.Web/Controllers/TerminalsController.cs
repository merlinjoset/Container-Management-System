using ContainerManagement.Application.Dtos.Terminals;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using ExcelDataReader;
using ClosedXML.Excel;

namespace ContainerManagement.Web.Controllers
{
    public class TerminalsController : Controller
    {
        private readonly TerminalService _terminalService;
        private readonly PortService _portService;

        public TerminalsController(TerminalService terminalService, PortService portService)
        {
            _terminalService = terminalService;
            _portService = portService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _terminalService.GetAllAsync(ct);
            await PopulatePortsAsync(null, ct);
            return View(list);
        }

        private async Task PopulatePortsAsync(Guid? selectedId, CancellationToken ct)
        {
            var ports = await _portService.GetAllAsync(ct);
            var items = ports
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(p.PortCode) ? p.FullName : $"{p.PortCode} - {p.FullName}",
                    Selected = selectedId.HasValue && p.Id == selectedId.Value
                })
                .ToList();
            ViewBag.Ports = items;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulatePortsAsync(null, ct);
            return View(new TerminalCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TerminalCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePortsAsync(dto.PortId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                await PopulatePortsAsync(dto.PortId, ct);
                return View(dto);
            }
            dto.CreatedBy = userId;

            try
            {
                await _terminalService.CreateAsync(dto, ct);
                TempData["Success"] = "Terminal created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulatePortsAsync(dto.PortId, ct);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var list = await _terminalService.GetAllAsync(ct);
            var t = list.FirstOrDefault(x => x.Id == id);
            if (t == null) return NotFound();

            var dto = new TerminalUpdateDto
            {
                Id = t.Id,
                PortId = t.PortId,
                TerminalName = t.TerminalName,
                TerminalCode = t.TerminalCode
            };

            await PopulatePortsAsync(dto.PortId, ct);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TerminalUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePortsAsync(dto.PortId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _terminalService.UpdateAsync(dto, ct);
                TempData["Success"] = "Terminal updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulatePortsAsync(dto.PortId, ct);
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

            await _terminalService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Terminal deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Import(CancellationToken ct)
        {
            await PopulatePortsAsync(null, ct);
            return View(new List<TerminalImportRowDto>());
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

            var previewRows = new List<TerminalImportRowDto>();
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
                        if (c0?.Contains("terminal") == true)
                        { rowIndex++; continue; }
                    }
                    var name = reader.FieldCount > 0 ? reader.GetValue(0)?.ToString()?.Trim() : null;
                    var code = reader.FieldCount > 1 ? reader.GetValue(1)?.ToString()?.Trim() : null;
                    var pcode = reader.FieldCount > 2 ? reader.GetValue(2)?.ToString()?.Trim() : null;

                    if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(code))
                    {
                        previewRows.Add(new TerminalImportRowDto
                        {
                            RowNumber = rowNum++,
                            TerminalName = name,
                            TerminalCode = code,
                            PortCode = pcode
                        });
                    }
                    rowIndex++;
                }
            }

            // Reject the file if any row has data in more than 3 columns
            if (maxNonEmptyCol > 2)
            {
                TempData["Error"] = $"The file has {maxNonEmptyCol + 1} columns with data. Terminal import accepts exactly 3 columns: Terminal Name (A), Terminal Code (B), Port Code (C). Please remove extra columns and try again.";
                return RedirectToAction(nameof(Import));
            }

            var ports = await _portService.GetAllAsync(ct);

            foreach (var row in previewRows)
            {
                if (string.IsNullOrWhiteSpace(row.TerminalName))
                    row.Errors.Add("Terminal Name is required.");

                if (string.IsNullOrWhiteSpace(row.PortCode))
                {
                    row.Errors.Add("Port Code is required.");
                }
                else
                {
                    var match = ports.FirstOrDefault(p =>
                        string.Equals(p.PortCode, row.PortCode, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                        row.PortId = match.Id;
                    else
                        row.Errors.Add($"Port Code '{row.PortCode}' not found in database.");
                }
            }

            var errorCount = previewRows.Count(r => r.HasErrors);
            if (errorCount > 0)
                ViewBag.ErrorSummary = $"{errorCount} row(s) have validation errors. Please correct them before importing.";

            await PopulatePortsAsync(null, ct);
            ViewBag.ShowPreview = true;
            return View(previewRows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport(List<TerminalImportRowDto> rows, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var ports = await _portService.GetAllAsync(ct);

            var importRows = new List<(string? TerminalName, string? TerminalCode, string? PortCode)>();
            foreach (var row in rows)
            {
                var portCode = ports.FirstOrDefault(p => p.Id == row.PortId)?.PortCode;
                importRows.Add((row.TerminalName, row.TerminalCode, portCode));
            }

            var (added, updated, skipped) = await _terminalService.ImportAsync(importRows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Terminals");
            ws.Cell(1, 1).Value = "Terminal Name";
            ws.Cell(1, 2).Value = "Terminal Code";
            ws.Cell(1, 3).Value = "Port Code";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns(1, 3).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "TerminalsTemplate.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineCreate([FromBody] TerminalCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.CreatedBy = userId;
            var id = await _terminalService.CreateAsync(dto, ct);
            var ports = await _portService.GetAllAsync(ct);
            var portName = ports.FirstOrDefault(p => p.Id == dto.PortId)?.FullName;
            return Ok(new { success = true, id, terminalName = dto.TerminalName, terminalCode = dto.TerminalCode, portName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] TerminalUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.ModifiedBy = userId;
            await _terminalService.UpdateAsync(dto, ct);
            var ports = await _portService.GetAllAsync(ct);
            var portName = ports.FirstOrDefault(p=>p.Id==dto.PortId)?.FullName;
            return Ok(new { success=true, terminalName=dto.TerminalName, terminalCode=dto.TerminalCode, portName });
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _terminalService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Terminals");
            ws.Cell(1,1).Value = "Terminal Name";
            ws.Cell(1,2).Value = "Terminal Code";
            ws.Cell(1,3).Value = "Port";
            ws.Range(1,1,1,3).Style.Font.Bold = true;
            ws.Range(1,1,1,3).Style.Fill.BackgroundColor = XLColor.LightGray;
            var r=2; foreach (var t in list){ ws.Cell(r,1).Value=t.TerminalName; ws.Cell(r,2).Value=t.TerminalCode; ws.Cell(r,3).Value=t.PortName; r++; }
            ws.Columns(1,3).AdjustToContents();
            using var ms = new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Terminals.xlsx");
        }
    }
}
