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

        public SchedulesController(PortService portService, TerminalService terminalService, VesselService vesselService, ServiceMasterService serviceMasterService, RouteMasterService routeMasterService)
        {
            _portService = portService;
            _terminalService = terminalService;
            _vesselService = vesselService;
            _serviceMasterService = serviceMasterService;
            _routeMasterService = routeMasterService;
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
    }
}
