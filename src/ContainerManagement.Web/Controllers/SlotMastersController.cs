using ContainerManagement.Application.Dtos.Slots;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ClosedXML.Excel;
using ExcelDataReader;

namespace ContainerManagement.Web.Controllers
{
    public class SlotMastersController : Controller
    {
        private readonly SlotMasterService _slotMasterService;

        public SlotMastersController(SlotMasterService slotMasterService)
        {
            _slotMasterService = slotMasterService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var slots = await _slotMasterService.GetAllAsync(ct);
            return View(slots);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            await _slotMasterService.DeleteAsync(id, userId, ct);
            TempData["Success"] = "Slot deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineCreate([FromBody] SlotCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.CreatedBy = userId;

            try
            {
                var id = await _slotMasterService.CreateAsync(dto, ct);
                return Ok(new { success = true, id, slotName = dto.SlotName });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InlineUpdate([FromBody] SlotUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            dto.ModifiedBy = userId;

            try
            {
                await _slotMasterService.UpdateAsync(dto, ct);
                return Ok(new { success = true, slotName = dto.SlotName });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Export(CancellationToken ct)
        {
            var list = await _slotMasterService.GetAllAsync(ct);
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Slots");
            ws.Cell(1, 1).Value = "Slot Name";
            ws.Range(1, 1, 1, 1).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            var r = 2;
            foreach (var item in list)
            {
                ws.Cell(r, 1).Value = item.SlotName;
                r++;
            }
            ws.Column(1).AdjustToContents();
            using var ms = new MemoryStream(); wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Slots.xlsx");
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

            var names = new List<string?>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                while (reader.Read())
                {
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("slot") == true) { rowIndex++; continue; }
                    }
                    names.Add(reader.GetValue(0)?.ToString());
                    rowIndex++;
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();

            var (added, updated, skipped) = await _slotMasterService.ImportAsync(names, userId, ct);
            TempData["Success"] = $"Import completed. Added: {added}, Skipped: {skipped}.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Template()
        {
            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Slots");
            ws.Cell(1, 1).Value = "Slot Name";
            ws.Range(1, 1, 1, 1).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 1).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Column(1).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "SlotsTemplate.xlsx");
        }
    }
}
