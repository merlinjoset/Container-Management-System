using ContainerManagement.Application.Dtos.Routes;
using ContainerManagement.Application.Dtos.RouteMasters;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using ExcelDataReader;
using ClosedXML.Excel;

namespace ContainerManagement.Web.Controllers
{
    public class RouteMastersController : Controller
    {
        private readonly RouteMasterService _routeMasterService;
        private readonly PortService _portService;

        public RouteMastersController(RouteMasterService routeMasterService, PortService portService)
        {
            _routeMasterService = routeMasterService;
            _portService = portService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var routes = await _routeMasterService.GetAllAsync(ct);
            await PopulatePortsAsync(null, null, ct);
            return View(routes);
        }

        private async Task PopulatePortsAsync(Guid? originId, Guid? destId, CancellationToken ct)
        {
            var ports = await _portService.GetAllAsync(ct);
            var items = ports
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(p.PortCode) ? p.FullName : $"({p.PortCode}) {p.FullName}"
                })
                .ToList();
            ViewBag.Ports = items;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulatePortsAsync(null, null, ct);
            return View(new RouteCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(RouteCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePortsAsync(dto.PortOfOriginId, dto.FinalDestinationId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                await PopulatePortsAsync(dto.PortOfOriginId, dto.FinalDestinationId, ct);
                return View(dto);
            }
            dto.CreatedBy = userId;

            try
            {
                await _routeMasterService.CreateAsync(dto, ct);
                TempData["Success"] = "Route created successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulatePortsAsync(dto.PortOfOriginId, dto.FinalDestinationId, ct);
                return View(dto);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var list = await _routeMasterService.GetAllAsync(ct);
            var route = list.FirstOrDefault(x => x.Id == id);
            if (route == null) return NotFound();

            var dto = new RouteUpdateDto
            {
                Id = route.Id,
                RouteName = route.RouteName,
                PortOfOriginId = route.PortOfOriginId,
                FinalDestinationId = route.FinalDestinationId
            };

            await PopulatePortsAsync(dto.PortOfOriginId, dto.FinalDestinationId, ct);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RouteUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulatePortsAsync(dto.PortOfOriginId, dto.FinalDestinationId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            dto.ModifiedBy = userId;

            try
            {
                await _routeMasterService.UpdateAsync(dto, ct);
                TempData["Success"] = "Route updated successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                await PopulatePortsAsync(dto.PortOfOriginId, dto.FinalDestinationId, ct);
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

            await _routeMasterService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Route deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Import(CancellationToken ct)
        {
            await PopulatePortsAsync(null, null, ct);
            return View(new List<RouteMasterImportRowDto>());
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

            var previewRows = new List<RouteMasterImportRowDto>();
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
                        if (c0?.Contains("route") == true)
                        { rowIndex++; continue; }
                    }
                    var routeName = reader.FieldCount > 0 ? reader.GetValue(0)?.ToString()?.Trim() : null;
                    var originCode = reader.FieldCount > 1 ? reader.GetValue(1)?.ToString()?.Trim() : null;
                    var destCode = reader.FieldCount > 2 ? reader.GetValue(2)?.ToString()?.Trim() : null;

                    if (!string.IsNullOrWhiteSpace(routeName) || !string.IsNullOrWhiteSpace(originCode) || !string.IsNullOrWhiteSpace(destCode))
                    {
                        previewRows.Add(new RouteMasterImportRowDto
                        {
                            RowNumber = rowNum++,
                            RouteName = routeName,
                            OriginCode = originCode,
                            DestCode = destCode
                        });
                    }
                    rowIndex++;
                }
            }

            // Match port codes to IDs from DB
            var ports = await _portService.GetAllAsync(ct);

            foreach (var row in previewRows)
            {
                if (string.IsNullOrWhiteSpace(row.RouteName))
                    row.Errors.Add("Route Name is required.");

                if (string.IsNullOrWhiteSpace(row.OriginCode))
                {
                    row.Errors.Add("Origin Code is required.");
                }
                else
                {
                    var match = ports.FirstOrDefault(p =>
                        string.Equals(p.PortCode, row.OriginCode, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                        row.OriginPortId = match.Id;
                    else
                        row.Errors.Add($"Origin Code '{row.OriginCode}' not found in database.");
                }

                if (string.IsNullOrWhiteSpace(row.DestCode))
                {
                    row.Errors.Add("Destination Code is required.");
                }
                else
                {
                    var match = ports.FirstOrDefault(p =>
                        string.Equals(p.PortCode, row.DestCode, StringComparison.OrdinalIgnoreCase));
                    if (match != null)
                        row.DestPortId = match.Id;
                    else
                        row.Errors.Add($"Destination Code '{row.DestCode}' not found in database.");
                }
            }

            var errorCount = previewRows.Count(r => r.HasErrors);
            if (errorCount > 0)
                ViewBag.ErrorSummary = $"{errorCount} row(s) have validation errors. Please correct them before importing.";

            await PopulatePortsAsync(null, null, ct);
            ViewBag.ShowPreview = true;
            return View(previewRows);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConfirmImport(List<RouteMasterImportRowDto> rows, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var ports = await _portService.GetAllAsync(ct);

            var importRows = new List<(string? RouteName, string? OriginCode, string? DestCode)>();
            foreach (var row in rows)
            {
                var originCode = ports.FirstOrDefault(p => p.Id == row.OriginPortId)?.PortCode;
                var destCode = ports.FirstOrDefault(p => p.Id == row.DestPortId)?.PortCode;
                importRows.Add((row.RouteName, originCode, destCode));
            }

            var (added, updated, skipped) = await _routeMasterService.ImportAsync(importRows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Routes");
            ws.Cell(1, 1).Value = "Route Name";
            ws.Cell(1, 2).Value = "Port of Origin Code";
            ws.Cell(1, 3).Value = "Final Destination Code";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns(1, 3).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "RoutesTemplate.xlsx");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineCreate([FromBody] RouteCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.CreatedBy = userId;
            var id = await _routeMasterService.CreateAsync(dto, ct);
            var ports = await _portService.GetAllAsync(ct);
            var originPort = ports.FirstOrDefault(p => p.Id == dto.PortOfOriginId);
            var destPort = ports.FirstOrDefault(p => p.Id == dto.FinalDestinationId);
            var originName = originPort != null ? (string.IsNullOrWhiteSpace(originPort.PortCode) ? originPort.FullName : $"({originPort.PortCode}) {originPort.FullName}") : "";
            var destName = destPort != null ? (string.IsNullOrWhiteSpace(destPort.PortCode) ? destPort.FullName : $"({destPort.PortCode}) {destPort.FullName}") : "";
            return Ok(new { success = true, id, routeName = dto.RouteName, portOfOriginName = originName, finalDestinationName = destName });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] RouteUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.ModifiedBy = userId;
            await _routeMasterService.UpdateAsync(dto, ct);
            var ports = await _portService.GetAllAsync(ct);
            var originPort = ports.FirstOrDefault(p => p.Id == dto.PortOfOriginId);
            var destPort = ports.FirstOrDefault(p => p.Id == dto.FinalDestinationId);
            var originName = originPort != null ? (string.IsNullOrWhiteSpace(originPort.PortCode) ? originPort.FullName : $"({originPort.PortCode}) {originPort.FullName}") : "";
            var destName = destPort != null ? (string.IsNullOrWhiteSpace(destPort.PortCode) ? destPort.FullName : $"({destPort.PortCode}) {destPort.FullName}") : "";
            return Ok(new { success = true, routeName = dto.RouteName, portOfOriginName = originName, finalDestinationName = destName });
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _routeMasterService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Routes");
            ws.Cell(1, 1).Value = "Route Name";
            ws.Cell(1, 2).Value = "Port of Origin";
            ws.Cell(1, 3).Value = "Final Destination";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            var r = 2;
            foreach (var item in list)
            {
                ws.Cell(r, 1).Value = item.RouteName;
                ws.Cell(r, 2).Value = item.PortOfOriginName;
                ws.Cell(r, 3).Value = item.FinalDestinationName;
                r++;
            }
            ws.Columns(1, 3).AdjustToContents();
            using var ms = new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Routes.xlsx");
        }
    }
}
