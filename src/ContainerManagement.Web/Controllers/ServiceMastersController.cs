using ContainerManagement.Application.Dtos.Services;
using ContainerManagement.Application.Dtos.ServiceMasters;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ExcelDataReader;
using ClosedXML.Excel;

namespace ContainerManagement.Web.Controllers
{
    public class ServiceMastersController : Controller
    {
        private readonly ServiceMasterService _serviceMasterService;

        public ServiceMastersController(ServiceMasterService serviceMasterService)
        {
            _serviceMasterService = serviceMasterService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var services = await _serviceMasterService.GetAllAsync(ct);
            return View(services);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View(new ServiceCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceCreateDto dto, CancellationToken ct)
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
                await _serviceMasterService.CreateAsync(dto, ct);
                TempData["Success"] = "Service created successfully.";
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
            var services = await _serviceMasterService.GetAllAsync(ct);
            var service = services.FirstOrDefault(x => x.Id == id);
            if (service == null)
                return NotFound();

            var dto = new ServiceUpdateDto
            {
                Id = service.Id,
                ServiceCode = service.ServiceCode,
                ServiceName = service.ServiceName
            };
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ServiceUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _serviceMasterService.UpdateAsync(dto, ct);
                TempData["Success"] = "Service updated successfully.";
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

            await _serviceMasterService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Service deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Import()
        {
            return View(new List<ServiceMasterImportRowDto>());
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

            var previewRows = new List<ServiceMasterImportRowDto>();
            int maxNonEmptyCol = -1;
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                var rowNum = 1;
                while (reader.Read())
                {
                    // Track max non-empty column index across the entire file
                    var fc = reader.FieldCount;
                    for (int c = 0; c < fc; c++)
                    {
                        var cellVal = reader.GetValue(c)?.ToString()?.Trim();
                        if (!string.IsNullOrWhiteSpace(cellVal) && c > maxNonEmptyCol)
                            maxNonEmptyCol = c;
                    }

                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("service") == true || c0?.Contains("name") == true)
                        { rowIndex++; continue; }
                    }
                    var name = reader.FieldCount > 0 ? reader.GetValue(0)?.ToString()?.Trim() : null;
                    var code = reader.FieldCount > 1 ? reader.GetValue(1)?.ToString()?.Trim() : null;

                    if (!string.IsNullOrWhiteSpace(name) || !string.IsNullOrWhiteSpace(code))
                    {
                        var row = new ServiceMasterImportRowDto
                        {
                            RowNumber = rowNum++,
                            ServiceName = name,
                            ServiceCode = code
                        };
                        if (string.IsNullOrWhiteSpace(name)) row.Errors.Add("Service Name is required.");
                        if (string.IsNullOrWhiteSpace(code)) row.Errors.Add("Service Code is required.");
                        previewRows.Add(row);
                    }
                    rowIndex++;
                }
            }

            // Reject the file if any row has data in more than 2 columns
            // (Service template has exactly 2 columns: Service Name, Service Code)
            if (maxNonEmptyCol > 1)
            {
                TempData["Error"] = $"The file has {maxNonEmptyCol + 1} columns with data. Service import accepts exactly 2 columns: Service Name (A) and Service Code (B). Please remove extra columns and try again.";
                return RedirectToAction(nameof(Import));
            }

            var errorCount = previewRows.Count(r => r.HasErrors);
            if (errorCount > 0)
                ViewBag.ErrorSummary = $"{errorCount} row(s) have validation errors.";

            ViewBag.ShowPreview = true;
            return View(previewRows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport(List<ServiceMasterImportRowDto> rows, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var importRows = rows
                .Where(r => !string.IsNullOrWhiteSpace(r.ServiceName))
                .Select(r => ((string?)r.ServiceName, (string?)r.ServiceCode));

            var (added, updated, skipped) = await _serviceMasterService.ImportAsync(importRows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Services");
            ws.Cell(1, 1).Value = "Service Name";
            ws.Cell(1, 2).Value = "Service Code";
            ws.Range(1, 1, 1, 2).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.Columns(1, 2).AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var bytes = ms.ToArray();
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(bytes, contentType, "ServicesTemplate.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineCreate([FromBody] ServiceCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.CreatedBy = userId;
            var id = await _serviceMasterService.CreateAsync(dto, ct);
            return Ok(new { success = true, id, serviceCode = dto.ServiceCode, serviceName = dto.ServiceName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] ServiceUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.ModifiedBy = userId;
            await _serviceMasterService.UpdateAsync(dto, ct);
            return Ok(new { success = true, serviceCode = dto.ServiceCode, serviceName = dto.ServiceName });
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _serviceMasterService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Services");
            ws.Cell(1, 1).Value = "Service Name";
            ws.Cell(1, 2).Value = "Service Code";
            ws.Range(1, 1, 1, 2).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 2).Style.Fill.BackgroundColor = XLColor.LightGray;
            var r = 2;
            foreach (var item in list) { ws.Cell(r, 1).Value = item.ServiceName; ws.Cell(r, 2).Value = item.ServiceCode; r++; }
            ws.Columns(1, 2).AdjustToContents();
            using var ms = new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Services.xlsx");
        }
    }
}
