using ContainerManagement.Application.Dtos.Voyages;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers
{
    public class VoyagesController : Controller
    {
        private readonly VoyageService _voyageService;
        private readonly VesselService _vesselService;
        private readonly ServiceMasterService _serviceMasterService;
        private readonly OperatorService _operatorService;
        private readonly PortService _portService;
        private readonly TerminalService _terminalService;

        public VoyagesController(
            VoyageService voyageService,
            VesselService vesselService,
            ServiceMasterService serviceMasterService,
            OperatorService operatorService,
            PortService portService,
            TerminalService terminalService)
        {
            _voyageService = voyageService;
            _vesselService = vesselService;
            _serviceMasterService = serviceMasterService;
            _operatorService = operatorService;
            _portService = portService;
            _terminalService = terminalService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var voyages = await _voyageService.GetAllAsync(ct);
            return View(voyages);
        }

        [HttpGet]
        public async Task<IActionResult> Create(CancellationToken ct)
        {
            await PopulateDropdownsAsync(ct);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromBody] VoyageCreateDto dto, CancellationToken ct)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized();

                dto.CreatedBy = userId;
                var id = await _voyageService.CreateAsync(dto, ct);
                return Json(new { success = true, id });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id, CancellationToken ct)
        {
            var voyage = await _voyageService.GetByIdWithPortsAsync(id, ct);
            if (voyage == null) return NotFound();

            var ports = await _voyageService.GetPortsByVoyageIdAsync(id, ct);
            ViewBag.VoyagePorts = ports;
            await PopulateDropdownsAsync(ct);
            return View(voyage);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromBody] VoyageUpdateDto dto, CancellationToken ct)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized();

                dto.ModifiedBy = userId;
                await _voyageService.UpdateAsync(dto, ct);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            await _voyageService.DeleteAsync(id, userId, ct);
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GetPorts(Guid id, CancellationToken ct)
        {
            var ports = await _voyageService.GetPortsByVoyageIdAsync(id, ct);
            return Json(ports);
        }

        private async Task PopulateDropdownsAsync(CancellationToken ct)
        {
            var vessels = await _vesselService.GetAllAsync(ct);
            ViewBag.Vessels = vessels.Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(v.VesselCode) ? v.VesselName : $"({v.VesselCode}) {v.VesselName}"
            }).ToList();

            var services = await _serviceMasterService.GetAllAsync(ct);
            ViewBag.Services = services.Select(s => new SelectListItem
            {
                Value = s.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(s.ServiceCode) ? s.ServiceName : $"({s.ServiceCode}) {s.ServiceName}"
            }).ToList();

            var operators = await _operatorService.GetAllAsync(ct);
            ViewBag.Operators = operators.Select(o => new SelectListItem
            {
                Value = o.Id.ToString(),
                Text = o.OperatorName
            }).ToList();

            var ports = await _portService.GetAllAsync(ct);
            ViewBag.Ports = ports.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(p.PortCode) ? p.FullName : $"({p.PortCode}) {p.FullName}"
            }).ToList();

            var terminals = await _terminalService.GetAllAsync(ct);
            ViewBag.Terminals = terminals.Select(t => new SelectListItem
            {
                Value = t.Id.ToString(),
                Text = string.IsNullOrWhiteSpace(t.TerminalCode) ? t.TerminalName : $"({t.TerminalCode}) {t.TerminalName}"
            }).ToList();
        }
    }
}
