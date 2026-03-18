using ContainerManagement.Application.Dtos.Distances;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using ClosedXML.Excel;
using ExcelDataReader;

namespace ContainerManagement.Web.Controllers
{
    public class DistanceMastersController : Controller
    {
        private readonly DistanceMasterService _distanceMasterService;
        private readonly PortService _portService;

        public DistanceMastersController(DistanceMasterService distanceMasterService, PortService portService)
        {
            _distanceMasterService = distanceMasterService;
            _portService = portService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var distances = await _distanceMasterService.GetAllAsync(ct);
            await PopulatePortsAsync(ct);
            return View(distances);
        }

        private async Task PopulatePortsAsync(CancellationToken ct)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            await _distanceMasterService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Distance deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineCreate([FromBody] DistanceCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.CreatedBy = userId;

            try
            {
                var id = await _distanceMasterService.CreateAsync(dto, ct);
                var ports = await _portService.GetAllAsync(ct);
                var fromPort = ports.FirstOrDefault(p => p.Id == dto.FromPortId);
                var toPort = ports.FirstOrDefault(p => p.Id == dto.ToPortId);
                var fromName = fromPort != null ? (string.IsNullOrWhiteSpace(fromPort.PortCode) ? fromPort.FullName : $"({fromPort.PortCode}) {fromPort.FullName}") : "";
                var toName = toPort != null ? (string.IsNullOrWhiteSpace(toPort.PortCode) ? toPort.FullName : $"({toPort.PortCode}) {toPort.FullName}") : "";
                return Ok(new { success = true, id, fromPortName = fromName, toPortName = toName, distance = dto.Distance });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] DistanceUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.ModifiedBy = userId;

            try
            {
                await _distanceMasterService.UpdateAsync(dto, ct);
                var ports = await _portService.GetAllAsync(ct);
                var fromPort = ports.FirstOrDefault(p => p.Id == dto.FromPortId);
                var toPort = ports.FirstOrDefault(p => p.Id == dto.ToPortId);
                var fromName = fromPort != null ? (string.IsNullOrWhiteSpace(fromPort.PortCode) ? fromPort.FullName : $"({fromPort.PortCode}) {fromPort.FullName}") : "";
                var toName = toPort != null ? (string.IsNullOrWhiteSpace(toPort.PortCode) ? toPort.FullName : $"({toPort.PortCode}) {toPort.FullName}") : "";
                return Ok(new { success = true, fromPortName = fromName, toPortName = toName, distance = dto.Distance });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _distanceMasterService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Distances");
            ws.Cell(1, 1).Value = "From Port";
            ws.Cell(1, 2).Value = "To Port";
            ws.Cell(1, 3).Value = "Distance";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            var r = 2;
            foreach (var item in list)
            {
                ws.Cell(r, 1).Value = item.FromPortName;
                ws.Cell(r, 2).Value = item.ToPortName;
                ws.Cell(r, 3).Value = item.Distance;
                r++;
            }
            ws.Columns(1, 3).AdjustToContents();
            using var ms = new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Distances.xlsx");
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

            var rows = new List<(string? FromPortCode, string? ToPortCode, decimal? Distance)>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                while (reader.Read())
                {
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("from") == true || c0?.Contains("port") == true) { rowIndex++; continue; }
                    }
                    string? S(int i) => reader.FieldCount > i ? reader.GetValue(i)?.ToString() : null;
                    decimal? Dec(int i) { if (reader.FieldCount <= i) return null; return decimal.TryParse(reader.GetValue(i)?.ToString(), out var v) ? v : null; }
                    rows.Add((S(0), S(1), Dec(2)));
                    rowIndex++;
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var (added, updated, skipped) = await _distanceMasterService.ImportAsync(rows, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Updated: {updated}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Distances");
            ws.Cell(1, 1).Value = "From Port Code";
            ws.Cell(1, 2).Value = "To Port Code";
            ws.Cell(1, 3).Value = "Distance";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns(1, 3).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "DistancesTemplate.xlsx");
        }
    }
}
