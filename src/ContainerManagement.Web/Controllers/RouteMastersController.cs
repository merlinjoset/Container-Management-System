using ContainerManagement.Application.Dtos.Routes;
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

            var rows = new List<(string? RouteName, string? OriginCode, string? DestCode)>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                while (reader.Read())
                {
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("route") == true)
                        { rowIndex++; continue; }
                    }
                    string? S(int i) => reader.FieldCount > i ? reader.GetValue(i)?.ToString() : null;
                    rows.Add((S(0), S(1), S(2)));
                    rowIndex++;
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            var (added, updated, skipped) = await _routeMasterService.ImportAsync(rows, userId, ct);
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
