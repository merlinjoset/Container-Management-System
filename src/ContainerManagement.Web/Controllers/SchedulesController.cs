using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ContainerManagement.Web.Controllers
{
    [Route("schedules")]
    public class SchedulesController : Controller
    {
        private readonly PortService _portService;
        private readonly TerminalService _terminalService;
        private readonly VesselService _vesselService;
        private readonly ServiceMasterService _serviceMasterService;
        private readonly RouteMasterService _routeMasterService;
        private readonly VoyageService _voyageService;

        public SchedulesController(
            PortService portService,
            TerminalService terminalService,
            VesselService vesselService,
            ServiceMasterService serviceMasterService,
            RouteMasterService routeMasterService,
            VoyageService voyageService)
        {
            _portService = portService;
            _terminalService = terminalService;
            _vesselService = vesselService;
            _serviceMasterService = serviceMasterService;
            _routeMasterService = routeMasterService;
            _voyageService = voyageService;
        }

        [HttpGet("viewer")]
        public async Task<IActionResult> Viewer(CancellationToken ct)
        {
            var ports = await _portService.GetAllAsync(ct);
            ViewBag.Ports = ports
                .Select(p => new SelectListItem
                {
                    Value = p.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(p.PortCode)
                        ? p.FullName
                        : $"({p.PortCode}){p.FullName}"
                })
                .ToList();

            var services = await _serviceMasterService.GetAllAsync(ct);
            ViewBag.Services = services
                .Select(s => new SelectListItem
                {
                    Value = s.Id.ToString(),
                    Text = string.IsNullOrWhiteSpace(s.ServiceCode)
                        ? s.ServiceName
                        : $"({s.ServiceCode}) {s.ServiceName}"
                })
                .ToList();

            var routes = await _routeMasterService.GetAllAsync(ct);
            ViewBag.Routes = routes
                .Select(r => new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.RouteName
                })
                .ToList();

            return View();
        }

        [HttpGet("api/schedule")]
        public async Task<IActionResult> GetSchedule(
            [FromQuery] DateTime? fromDate,
            [FromQuery] DateTime? toDate,
            [FromQuery] Guid? serviceId,
            [FromQuery] Guid? portId,
            [FromQuery] Guid? polId,
            [FromQuery] Guid? podId,
            [FromQuery] Guid? routeId,
            CancellationToken ct)
        {
            try
            {
                // If routeId is provided, resolve to POL/POD
                if (routeId.HasValue)
                {
                    var routes = await _routeMasterService.GetAllAsync(ct);
                    var route = routes.FirstOrDefault(r => r.Id == routeId.Value);
                    if (route != null)
                    {
                        polId = route.PortOfOriginId;
                        podId = route.FinalDestinationId;
                    }
                }

                var rows = await _voyageService.GetScheduleRowsAsync(
                    fromDate, toDate, serviceId, portId, polId, podId, ct);

                return Json(rows);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }
}
