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
        private readonly CountryService _countryService;

        public OperatorsController(OperatorService operatorService, VendorService vendorService, CountryService countryService)
        {
            _operatorService = operatorService;
            _vendorService = vendorService;
            _countryService = countryService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _operatorService.GetAllAsync(ct);
            return View(list);
        }

        private async Task PopulateLookupsAsync(Guid? vendorId, Guid? countryId, CancellationToken ct)
        {
            var vendors = await _vendorService.GetAllAsync(ct);
            var vendorItems = vendors.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.VendorCode} - {v.VendorName}",
                Selected = vendorId.HasValue && v.Id == vendorId.Value
            }).ToList();

            var countries = await _countryService.GetAllAsync(ct);
            var countryItems = countries.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(c.CountryCode) ? c.CountryName : $"{c.CountryCode} - {c.CountryName}",
                Selected = countryId.HasValue && c.Id == countryId.Value
            }).ToList();

            ViewBag.Vendors = vendorItems;
            ViewBag.Countries = countryItems;
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateLookupsAsync(null, null, ct);
            return View(new OperatorCreateDto());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(OperatorCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
                return View(dto);
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
            {
                ModelState.AddModelError("", "Invalid user session. Please login again.");
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
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
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
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
                CountryId = op.CountryId
            };

            await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(OperatorUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
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
                await PopulateLookupsAsync(dto.VendorId, dto.CountryId, ct);
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

            var rows = new List<(string? OperatorName, string? VendorCode, string? CountryCode)>();
            using (var stream = file.OpenReadStream())
            using (var reader = ext == ".xls" ? ExcelReaderFactory.CreateBinaryReader(stream) : ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                var rowIndex = 0;
                while (reader.Read())
                {
                    if (rowIndex == 0)
                    {
                        var c0 = reader.GetValue(0)?.ToString()?.Trim().ToLowerInvariant();
                        if (c0?.Contains("operator") == true)
                        { rowIndex++; continue; }
                    }
                    string? S(int i) => reader.FieldCount > i ? reader.GetValue(i)?.ToString() : null;
                    rows.Add((S(0), S(1), S(2)));
                    rowIndex++;
                }
            }

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId)) return Unauthorized();
            var (added, updated, skipped) = await _operatorService.ImportAsync(rows, userId, ct);
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
            ws.Cell(1, 3).Value = "Country Code";
            ws.Range(1, 1, 1, 3).Style.Font.Bold = true;
            ws.Range(1, 1, 1, 3).Style.Fill.BackgroundColor = XLColor.LightGray;
            ws.Columns(1, 3).AdjustToContents();
            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "OperatorsTemplate.xlsx");
        }
    }
}
