using ContainerManagement.Application.Dtos.Operators;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using ExcelDataReader;
using ClosedXML.Excel;

namespace ContainerManagement.Web.Controllers
{
    public class OperatorsController : Controller
    {
        private readonly OperatorService _operatorService;
        private readonly VendorService _vendorService;

        public OperatorsController(OperatorService operatorService, VendorService vendorService)
        {
            _operatorService = operatorService;
            _vendorService = vendorService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _operatorService.GetAllAsync(ct);
            await PopulateLookupsAsync(null, ct);
            return View(list);
        }

        // Route to serve the UI-style Operator page at /Operator (plural controller)
        [HttpGet("/Operator")]
        public IActionResult Operator()
        {
            return RedirectToAction(nameof(Index));
        }

        private async Task PopulateLookupsAsync(Guid? vendorId, CancellationToken ct)
        {
            var vendors = await _vendorService.GetAllAsync(ct);
            var vendorItems = vendors.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.VendorCode} - {v.VendorName}",
                Selected = vendorId.HasValue && v.Id == vendorId.Value
            }).ToList();

            ViewBag.Vendors = vendorItems;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateLookupsAsync(null, ct);
            return View(new OperatorCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OperatorCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(dto.VendorId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                await PopulateLookupsAsync(dto.VendorId, ct);
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
                await PopulateLookupsAsync(dto.VendorId, ct);
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
                VendorId = op.VendorId,
                IsCompetitor = op.IsCompetitor
            };

            await PopulateLookupsAsync(dto.VendorId, ct);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OperatorUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(dto.VendorId, ct);
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
                await PopulateLookupsAsync(dto.VendorId, ct);
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

        [HttpGet]
        public async Task<IActionResult> Import(CancellationToken ct)
        {
            await PopulateLookupsAsync(null, ct);
            return View(new List<OperatorImportRowDto>());
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

            var previewRows = new List<OperatorImportRowDto>();
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
                        if (c0?.Contains("operator") == true)
                        { rowIndex++; continue; }
                    }
                    var name = reader.FieldCount > 0 ? reader.GetValue(0)?.ToString()?.Trim() : null;
                    var vcode = reader.FieldCount > 1 ? reader.GetValue(1)?.ToString()?.Trim() : null;
                    var compText = reader.FieldCount > 2 ? reader.GetValue(2)?.ToString()?.Trim() : null;

                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        var isComp = ParseYesNo(compText);
                        previewRows.Add(new OperatorImportRowDto
                        {
                            RowNumber = rowNum++,
                            OperatorName = name,
                            VendorCode = vcode,
                            IsCompetitorText = compText,
                            IsCompetitor = isComp
                        });
                    }
                    rowIndex++;
                }
            }

            var vendors = await _vendorService.GetAllAsync(ct);

            foreach (var row in previewRows)
            {
                if (string.IsNullOrWhiteSpace(row.OperatorName))
                    row.Errors.Add("Operator Name is required.");

                if (!string.IsNullOrWhiteSpace(row.VendorCode))
                {
                    var match = vendors.FirstOrDefault(v =>
                        string.Equals(v.VendorCode, row.VendorCode, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                        row.VendorId = match.Id;
                    else
                        row.Errors.Add($"Vendor Code '{row.VendorCode}' not found in database.");
                }
            }

            var errorCount = previewRows.Count(r => r.HasErrors);
            if (errorCount > 0)
                ViewBag.ErrorSummary = $"{errorCount} row(s) have validation errors. Please correct them before importing.";

            await PopulateLookupsAsync(null, ct);
            ViewBag.ShowPreview = true;
            return View(previewRows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport(List<OperatorImportRowDto> rows, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var vendors = await _vendorService.GetAllAsync(ct);

            var importRows = new List<(string? OperatorName, string? VendorCode, bool IsCompetitor)>();
            foreach (var row in rows)
            {
                var vendorCode = vendors.FirstOrDefault(v => v.Id == row.VendorId)?.VendorCode;
                importRows.Add((row.OperatorName, vendorCode, row.IsCompetitor));
            }

            var (added, updated, skipped) = await _operatorService.ImportAsync(importRows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Operators");
            ws.Cell(1, 1).Value = "Operator Name";
            ws.Cell(1, 2).Value = "Vendor Code";
            ws.Cell(1, 3).Value = "Competitor (Yes/No)";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns(1, 3).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OperatorsTemplate.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineCreate([FromBody] OperatorCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.CreatedBy = userId;
            var id = await _operatorService.CreateAsync(dto, ct);
            var vendors = await _vendorService.GetAllAsync(ct);
            var vendorName = vendors.FirstOrDefault(v => v.Id == dto.VendorId)?.VendorName;
            return Ok(new { success = true, id, operatorName = dto.OperatorName, vendorName, isCompetitor = dto.IsCompetitor });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] OperatorUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;
            await _operatorService.UpdateAsync(dto, ct);

            var vendors = await _vendorService.GetAllAsync(ct);
            var vendorName = vendors.FirstOrDefault(v => v.Id == dto.VendorId)?.VendorName;

            return Ok(new { success = true, operatorName = dto.OperatorName, vendorName, isCompetitor = dto.IsCompetitor });
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _operatorService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Operators");
            ws.Cell(1, 1).Value = "Operator Name";
            ws.Cell(1, 2).Value = "Vendor";
            ws.Cell(1, 3).Value = "Competitor";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            var row = 2;
            foreach (var item in list)
            {
                ws.Cell(row, 1).Value = item.OperatorName;
                ws.Cell(row, 2).Value = item.VendorName;
                ws.Cell(row, 3).Value = item.IsCompetitor ? "Yes" : "No";
                row++;
            }
            ws.Columns(1, 3).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Operators.xlsx");
        }

        private static bool ParseYesNo(string? text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            var t = text.Trim().ToLowerInvariant();
            return t == "yes" || t == "y" || t == "true" || t == "1";
        }
    }
}
