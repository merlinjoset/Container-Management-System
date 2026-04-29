using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Voyages;
using ContainerManagement.Application.Services;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Configuration;
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
        private readonly DistanceMasterService _distanceMasterService;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public VoyagesController(
            VoyageService voyageService,
            VesselService vesselService,
            ServiceMasterService serviceMasterService,
            OperatorService operatorService,
            PortService portService,
            TerminalService terminalService,
            DistanceMasterService distanceMasterService,
            IEmailSender emailSender,
            IConfiguration config)
        {
            _voyageService = voyageService;
            _vesselService = vesselService;
            _serviceMasterService = serviceMasterService;
            _operatorService = operatorService;
            _portService = portService;
            _terminalService = terminalService;
            _distanceMasterService = distanceMasterService;
            _emailSender = emailSender;
            _config = config;
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

        [HttpGet]
        public async Task<IActionResult> Arrival(Guid id, CancellationToken ct)
        {
            // id = VoyagePortId
            var voyagePorts = await _voyageService.GetPortsByVoyageIdAsync(Guid.Empty, ct);

            // Find the VoyagePort to get context
            // We need to look up via all voyages - get the port directly
            var allVoyages = await _voyageService.GetAllAsync(ct);
            VoyagePortListItemDto? portItem = null;
            VoyageListItemDto? parentVoyage = null;

            foreach (var v in allVoyages)
            {
                var ports = await _voyageService.GetPortsByVoyageIdAsync(v.Id, ct);
                portItem = ports.FirstOrDefault(p => p.Id == id);
                if (portItem != null) { parentVoyage = v; break; }
            }

            if (portItem == null) return NotFound();

            var arrival = await _voyageService.GetArrivalByVoyagePortIdAsync(id, ct);

            ViewBag.VoyagePort = portItem;
            ViewBag.ParentVoyage = parentVoyage;
            await PopulateDropdownsAsync(ct);

            return View(arrival ?? new VoyagePortArrivalDto { VoyagePortId = id });
        }

        [HttpPost]
        public async Task<IActionResult> Arrival([FromBody] VoyagePortArrivalDto dto, CancellationToken ct)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                dto.CreatedBy = userId;
                dto.ModifiedBy = userId;
                var arrivalId = await _voyageService.SaveArrivalAsync(dto, ct);
                return Json(new { success = true, id = arrivalId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetArrival(Guid voyagePortId, CancellationToken ct)
        {
            var arrival = await _voyageService.GetArrivalByVoyagePortIdAsync(voyagePortId, ct);
            return Json(arrival);
        }

        [HttpGet]
        public async Task<IActionResult> Departure(Guid id, CancellationToken ct)
        {
            var allVoyages = await _voyageService.GetAllAsync(ct);
            VoyagePortListItemDto? portItem = null;
            VoyageListItemDto? parentVoyage = null;

            foreach (var v in allVoyages)
            {
                var ports = await _voyageService.GetPortsByVoyageIdAsync(v.Id, ct);
                portItem = ports.FirstOrDefault(p => p.Id == id);
                if (portItem != null) { parentVoyage = v; break; }
            }

            if (portItem == null) return NotFound();

            var departure = await _voyageService.GetDepartureByVoyagePortIdAsync(id, ct);

            // For new departures, still load arrival bunkers for validation baseline
            if (departure == null)
            {
                departure = new VoyagePortDepartureDto { VoyagePortId = id };
                var arrivalBunkers = await _voyageService.GetArrivalBunkersByVoyagePortIdAsync(id, ct);
                departure.ArrivalBunkers = arrivalBunkers;
            }

            ViewBag.VoyagePort = portItem;
            ViewBag.ParentVoyage = parentVoyage;
            await PopulateDropdownsAsync(ct);

            return View(departure);
        }

        [HttpPost]
        public async Task<IActionResult> Departure([FromBody] VoyagePortDepartureDto dto, CancellationToken ct)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                dto.CreatedBy = userId;
                dto.ModifiedBy = userId;
                var depId = await _voyageService.SaveDepartureAsync(dto, ct);
                return Json(new { success = true, id = depId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── TOS (Terminal Operating System) ──

        [HttpGet]
        public async Task<IActionResult> Tos(Guid id, CancellationToken ct)
        {
            var allVoyages = await _voyageService.GetAllAsync(ct);
            VoyagePortListItemDto? portItem = null;
            VoyageListItemDto? parentVoyage = null;

            foreach (var v in allVoyages)
            {
                var ports = await _voyageService.GetPortsByVoyageIdAsync(v.Id, ct);
                portItem = ports.FirstOrDefault(p => p.Id == id);
                if (portItem != null) { parentVoyage = v; break; }
            }

            if (portItem == null) return NotFound();

            var tos = await _voyageService.GetTosByVoyagePortIdAsync(id, ct);

            // Load arrival/departure for Total Ops Time calculation
            var arrival = await _voyageService.GetArrivalByVoyagePortIdAsync(id, ct);
            var departure = await _voyageService.GetDepartureByVoyagePortIdAsync(id, ct);

            ViewBag.VoyagePort = portItem;
            ViewBag.ParentVoyage = parentVoyage;
            ViewBag.CommencedCargoOperation = arrival?.CommencedCargoOperation;
            ViewBag.CompleteCargoOperation = departure?.CompleteCargoOperation;
            ViewBag.ActualETA = arrival?.ActualETA;
            ViewBag.ActualETB = arrival?.ActualETB;
            ViewBag.ActualETD = departure?.ActualETD;
            ViewBag.ArrivalPilotOnBoard = arrival?.PilotOnBoard;

            return View(tos ?? new TosDto { VoyagePortId = id });
        }

        [HttpPost]
        public async Task<IActionResult> Tos([FromBody] TosDto dto, CancellationToken ct)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized(Json(new { success = false, message = "Invalid session." }));

                dto.CreatedBy = userId;
                dto.ModifiedBy = userId;

                var tosId = await _voyageService.SaveTosAsync(dto, ct);
                return Json(new { success = true, id = tosId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ── Edit Request Workflow ──
        // When TOS is saved, the entire row (Arrival, Departure, TOS) is locked in the UI.
        // Users can submit an edit request that an admin must approve. Email send is a stub
        // (logged to console) — wire to SMTP/SendGrid later.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RequestEditApproval([FromBody] EditRequestDto dto, CancellationToken ct)
        {
            try
            {
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (!Guid.TryParse(userIdStr, out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                var requestId = await _voyageService.SubmitEditRequestAsync(dto, userId, ct);

                // Resolve port context for the email body
                string portCode = "—", vesselName = "—", serviceCode = "—";
                try
                {
                    var allVoyages = await _voyageService.GetAllAsync(ct);
                    foreach (var v in allVoyages)
                    {
                        var ports = await _voyageService.GetPortsByVoyageIdAsync(v.Id, ct);
                        var portItem = ports.FirstOrDefault(p => p.Id == dto.VoyagePortId);
                        if (portItem != null)
                        {
                            portCode = portItem.PortCode ?? "—";
                            vesselName = v.VesselName ?? "—";
                            serviceCode = v.ServiceCode ?? "—";
                            break;
                        }
                    }
                }
                catch { /* non-fatal — email body falls back to ids */ }

                var requesterEmail = User.FindFirstValue(ClaimTypes.Email) ?? "Unknown";
                var requesterName = User.Identity?.Name ?? requesterEmail;
                var adminAddr = _config["Email:AdminAddress"] ?? string.Empty;

                var subject = $"[Edit Request] {dto.ReportType} — {vesselName} / {portCode} (Request #{requestId.ToString().Substring(0, 8)})";
                var body = $@"
                    <h3 style='font-family:sans-serif;color:#0f172a;'>Edit Approval Request</h3>
                    <p style='font-family:sans-serif;font-size:14px;color:#334155;'>
                        A user has requested permission to edit a locked report. Please review and approve or reject.
                    </p>
                    <table style='font-family:sans-serif;font-size:13px;border-collapse:collapse;'>
                        <tr><td style='padding:4px 12px;color:#64748b;'>Requester:</td><td style='padding:4px 12px;'><strong>{System.Net.WebUtility.HtmlEncode(requesterName)}</strong> ({System.Net.WebUtility.HtmlEncode(requesterEmail)})</td></tr>
                        <tr><td style='padding:4px 12px;color:#64748b;'>Vessel:</td><td style='padding:4px 12px;'>{System.Net.WebUtility.HtmlEncode(vesselName)}</td></tr>
                        <tr><td style='padding:4px 12px;color:#64748b;'>Service:</td><td style='padding:4px 12px;'>{System.Net.WebUtility.HtmlEncode(serviceCode)}</td></tr>
                        <tr><td style='padding:4px 12px;color:#64748b;'>Port:</td><td style='padding:4px 12px;'>{System.Net.WebUtility.HtmlEncode(portCode)}</td></tr>
                        <tr><td style='padding:4px 12px;color:#64748b;'>Report:</td><td style='padding:4px 12px;'><strong>{System.Net.WebUtility.HtmlEncode(dto.ReportType)}</strong></td></tr>
                        <tr><td style='padding:4px 12px;color:#64748b;vertical-align:top;'>Reason:</td><td style='padding:4px 12px;'>{System.Net.WebUtility.HtmlEncode(dto.Reason)}</td></tr>
                        <tr><td style='padding:4px 12px;color:#64748b;'>Request ID:</td><td style='padding:4px 12px;font-family:monospace;'>{requestId}</td></tr>
                        <tr><td style='padding:4px 12px;color:#64748b;'>Submitted:</td><td style='padding:4px 12px;'>{DateTime.UtcNow:dd MMM yyyy HH:mm} UTC</td></tr>
                    </table>";

                bool emailSent = false;
                if (!string.IsNullOrWhiteSpace(adminAddr))
                {
                    emailSent = await _emailSender.SendAsync(adminAddr, subject, body, ct);
                }

                var msg = emailSent
                    ? "Edit request submitted and email sent to admin."
                    : "Edit request recorded. (Email could not be sent — admin will see it in the dashboard.)";

                return Ok(new { success = true, id = requestId, emailSent, message = msg });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> ExportTos(Guid id, CancellationToken ct)
        {
            // id = VoyagePortId
            var tos = await _voyageService.GetTosByVoyagePortIdAsync(id, ct);

            // Fetch context for header (vessel/service/port)
            var allVoyages = await _voyageService.GetAllAsync(ct);
            VoyagePortListItemDto? portItem = null;
            VoyageListItemDto? parentVoyage = null;
            foreach (var v in allVoyages)
            {
                var ports = await _voyageService.GetPortsByVoyageIdAsync(v.Id, ct);
                portItem = ports.FirstOrDefault(p => p.Id == id);
                if (portItem != null) { parentVoyage = v; break; }
            }

            // Pull arrival for Actual ETA/ETB
            var exportArrival = await _voyageService.GetArrivalByVoyagePortIdAsync(id, ct);

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("TOS Report");

            // Column widths
            ws.Column(1).Width = 32;
            ws.Column(2).Width = 22;
            ws.Column(3).Width = 22;
            ws.Column(4).Width = 22;
            ws.Column(5).Width = 30;

            int row = 1;

            // Title
            ws.Cell(row, 1).Value = "Terminal Operating System (TOS)";
            ws.Range(row, 1, row, 5).Merge();
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            ws.Cell(row, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            row += 2;

            // Context
            ws.Cell(row, 1).Value = "Vessel:"; ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = parentVoyage?.VesselName ?? "—";
            row++;
            ws.Cell(row, 1).Value = "Service:"; ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = parentVoyage?.ServiceCode ?? "—";
            row++;
            ws.Cell(row, 1).Value = "Port:"; ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = portItem?.PortCode ?? "—";
            row++;
            ws.Cell(row, 1).Value = "Terminal:"; ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Value = portItem?.TerminalCode ?? "—";
            row++;
            ws.Cell(row, 1).Value = "Actual ETA:"; ws.Cell(row, 1).Style.Font.Bold = true;
            if (exportArrival?.ActualETA.HasValue == true)
            {
                ws.Cell(row, 2).Value = exportArrival.ActualETA.Value;
                ws.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            }
            row++;
            ws.Cell(row, 1).Value = "Actual ETB:"; ws.Cell(row, 1).Style.Font.Bold = true;
            if (exportArrival?.ActualETB.HasValue == true)
            {
                ws.Cell(row, 2).Value = exportArrival.ActualETB.Value;
                ws.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
            }
            row += 2;

            // Section 1: Stoppages
            ws.Cell(row, 1).Value = "Add Operation stoppage time";
            ws.Cell(row, 2).Value = "from dd/hrs";
            ws.Cell(row, 3).Value = "to dd/hrs";
            ws.Cell(row, 4).Value = "Non-Productive Time(hrs)";
            ws.Cell(row, 5).Value = "Reason";
            ws.Range(row, 1, row, 5).Style.Font.Bold = true;
            ws.Range(row, 1, row, 5).Style.Fill.BackgroundColor = XLColor.LightGray;
            row++;

            var stoppages = tos?.Stoppages ?? new List<TosStoppageDto>();
            for (int i = 0; i < stoppages.Count; i++)
            {
                var s = stoppages[i];
                ws.Cell(row, 1).Value = i + 1;
                if (s.FromDateTime.HasValue) { ws.Cell(row, 2).Value = s.FromDateTime.Value; ws.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm"; }
                if (s.ToDateTime.HasValue) { ws.Cell(row, 3).Value = s.ToDateTime.Value; ws.Cell(row, 3).Style.DateFormat.Format = "dd/MM/yyyy HH:mm"; }
                if (s.NonProductiveHours.HasValue) { ws.Cell(row, 4).Value = s.NonProductiveHours.Value; ws.Cell(row, 4).Style.NumberFormat.Format = "0.00"; }
                ws.Cell(row, 5).Value = s.Reason ?? "";
                row++;
            }
            row++;

            // Section 2: Container Moves
            ws.Cell(row, 2).Value = "No of Units";
            ws.Cell(row, 2).Style.Font.Bold = true;
            row++;
            ws.Cell(row, 1).Value = "Discharge moves"; ws.Cell(row, 2).Value = tos?.DischargeMoves; row++;
            ws.Cell(row, 1).Value = "Load Moves"; ws.Cell(row, 2).Value = tos?.LoadMoves; row++;
            ws.Cell(row, 1).Value = "No of restows"; ws.Cell(row, 2).Value = tos?.Restows; row++;
            ws.Cell(row, 1).Value = "Total Hatch Cover"; ws.Cell(row, 2).Value = tos?.TotalHatchCover; row++;
            ws.Cell(row, 1).Value = "no of bin"; ws.Cell(row, 2).Value = tos?.NoOfBin; row++;
            ws.Cell(row, 1).Value = "Total Moves"; ws.Cell(row, 2).Value = tos?.TotalMoves;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 2).Style.Font.Bold = true;
            row += 2;

            // Section 3: Crane Productivity
            ws.Cell(row, 1).Value = "Crane Productivity";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            row += 2;
            var cp = tos?.CraneProductivity;
            ws.Cell(row, 1).Value = "No of Cranes as per SI"; ws.Cell(row, 2).Value = cp?.CranesPerSI; row++;
            ws.Cell(row, 1).Value = "Actual Cranes"; ws.Cell(row, 2).Value = cp?.ActualCranes; row++;
            ws.Cell(row, 1).Value = "Total Ops time(hrs)"; if (cp?.TotalOpsTime.HasValue == true) { ws.Cell(row, 2).Value = cp.TotalOpsTime.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Non-Productive Time"; if (cp?.NonProductiveTime.HasValue == true) { ws.Cell(row, 2).Value = cp.NonProductiveTime.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Crane working hrs"; if (cp?.CraneWorkingHrs.HasValue == true) { ws.Cell(row, 2).Value = cp.CraneWorkingHrs.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Crane Productivity per hr"; if (cp?.CraneProductivityPerHr.HasValue == true) { ws.Cell(row, 2).Value = cp.CraneProductivityPerHr.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Moves /Crane"; if (cp?.MovesPerCrane.HasValue == true) { ws.Cell(row, 2).Value = cp.MovesPerCrane.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            row++;

            // Section 4: Ship Productivity
            ws.Cell(row, 1).Value = "Ship Productivity";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            row++;
            var sp = tos?.ShipProductivity;
            ws.Cell(row, 1).Value = "Port stay Time"; if (sp?.PortStayTime.HasValue == true) { ws.Cell(row, 2).Value = sp.PortStayTime.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Productivity per hr"; if (sp?.ProductivityPerHr.HasValue == true) { ws.Cell(row, 2).Value = sp.ProductivityPerHr.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Moves /Crane"; if (sp?.MovesPerCrane.HasValue == true) { ws.Cell(row, 2).Value = sp.MovesPerCrane.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            row++;

            // Section 5: Summary
            ws.Cell(row, 1).Value = "Summary";
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            row++;
            ws.Cell(row, 2).Value = "Hrs";
            ws.Cell(row, 2).Style.Font.Bold = true;
            row++;
            var sm = tos?.Summary;
            ws.Cell(row, 1).Value = "Vessel Turnaround Time:"; if (sm?.VesselTurnaroundTime.HasValue == true) { ws.Cell(row, 2).Value = sm.VesselTurnaroundTime.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Berthing Delay"; if (sm?.BerthingDelay.HasValue == true) { ws.Cell(row, 2).Value = sm.BerthingDelay.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Total Moves"; ws.Cell(row, 2).Value = sm?.TotalMoves ?? tos?.TotalMoves; row++;
            ws.Cell(row, 1).Value = "Non-Productive Time"; if (sm?.NonProductiveTime.HasValue == true) { ws.Cell(row, 2).Value = sm.NonProductiveTime.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Terminal Productivity"; if (sm?.TerminalProductivity.HasValue == true) { ws.Cell(row, 2).Value = sm.TerminalProductivity.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;
            ws.Cell(row, 1).Value = "Ship Productivity"; if (sm?.ShipProductivity.HasValue == true) { ws.Cell(row, 2).Value = sm.ShipProductivity.Value; ws.Cell(row, 2).Style.NumberFormat.Format = "0.00"; } row++;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var fileName = $"TOS_{portItem?.PortCode}_{parentVoyage?.VesselName}_{DateTime.Now:yyyyMMdd}.xlsx".Replace(" ", "_");
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportPorts(Guid id, CancellationToken ct)
        {
            // id = VoyageId
            var voyage = await _voyageService.GetByIdWithPortsAsync(id, ct);
            if (voyage == null) return NotFound();

            var ports = await _voyageService.GetPortsByVoyageIdAsync(id, ct);

            using var wb = new XLWorkbook();

            // --- Sheet 1: Port Rotation Summary ---
            var ws1 = wb.AddWorksheet("Port Rotation");
            // Voyage info header
            ws1.Cell(1, 1).Value = "Vessel";   ws1.Cell(1, 2).Value = voyage.VesselName;
            ws1.Cell(2, 1).Value = "Service";   ws1.Cell(2, 2).Value = voyage.ServiceCode;
            ws1.Cell(3, 1).Value = "Operator";  ws1.Cell(3, 2).Value = voyage.OperatorName;
            ws1.Range(1, 1, 3, 1).Style.Font.Bold = true;

            var row = 5;
            var portHeaders = new[] { "#", "Voy No", "Bound", "Port", "Terminal", "ETA", "ETB", "ETD", "Actual ETA", "Actual ETB", "Actual ETD", "Port Stay", "Speed", "Distance", "Sea Day" };
            for (int i = 0; i < portHeaders.Length; i++)
                ws1.Cell(row, i + 1).Value = portHeaders[i];
            ws1.Range(row, 1, row, portHeaders.Length).Style.Font.Bold = true;
            ws1.Range(row, 1, row, portHeaders.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#312e81");
            ws1.Range(row, 1, row, portHeaders.Length).Style.Font.FontColor = XLColor.White;
            row++;

            string Fmt(DateTime? dt) => dt?.ToString("dd/MM/yyyy HH:mm") ?? "";
            foreach (var p in ports)
            {
                var idx = ports.IndexOf(p) + 1;
                ws1.Cell(row, 1).Value = idx;
                ws1.Cell(row, 2).Value = p.VoyNo;
                ws1.Cell(row, 3).Value = p.Bound;
                ws1.Cell(row, 4).Value = p.PortCode;
                ws1.Cell(row, 5).Value = p.TerminalCode;
                ws1.Cell(row, 6).Value = Fmt(p.ETA);
                ws1.Cell(row, 7).Value = Fmt(p.ETB);
                ws1.Cell(row, 8).Value = Fmt(p.ETD);
                ws1.Cell(row, 9).Value = Fmt(p.ActualETA);
                ws1.Cell(row, 10).Value = Fmt(p.ActualETB);
                ws1.Cell(row, 11).Value = Fmt(p.ActualETD);
                ws1.Cell(row, 12).Value = p.PortStay;
                ws1.Cell(row, 13).Value = p.Speed;
                ws1.Cell(row, 14).Value = p.Distance;
                ws1.Cell(row, 15).Value = p.SeaDay;

                // Highlight actual dates green
                if (p.HasArrival)
                {
                    ws1.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#dcfce7");
                    ws1.Cell(row, 10).Style.Fill.BackgroundColor = XLColor.FromHtml("#dcfce7");
                }
                if (p.HasDeparture)
                    ws1.Cell(row, 11).Style.Fill.BackgroundColor = XLColor.FromHtml("#fde8d8");

                row++;
            }
            ws1.Columns(1, portHeaders.Length).AdjustToContents();

            // --- Sheet 2: Arrival Reports ---
            var ws2 = wb.AddWorksheet("Arrival Reports");
            var arrHeaders = new[] { "Port", "Voy No", "Actual ETA", "Actual ETB", "Pilot On Board", "Commenced Cargo Ops",
                "Tugs In", "Draft Fwd(m)", "Draft Aft(m)", "Draft Mean(m)",
                "Fresh Water", "Ballast Water", "Remarks" };
            for (int i = 0; i < arrHeaders.Length; i++)
                ws2.Cell(1, i + 1).Value = arrHeaders[i];
            ws2.Range(1, 1, 1, arrHeaders.Length).Style.Font.Bold = true;
            ws2.Range(1, 1, 1, arrHeaders.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#1e3a8a");
            ws2.Range(1, 1, 1, arrHeaders.Length).Style.Font.FontColor = XLColor.White;
            row = 2;
            foreach (var p in ports)
            {
                ws2.Cell(row, 1).Value = p.PortCode;
                ws2.Cell(row, 2).Value = p.VoyNo;
                ws2.Cell(row, 3).Value = Fmt(p.ActualETA);
                ws2.Cell(row, 4).Value = Fmt(p.ActualETB);
                ws2.Cell(row, 5).Value = Fmt(p.ArrPilotOnBoard);
                ws2.Cell(row, 6).Value = Fmt(p.ArrCommencedCargoOp);
                ws2.Cell(row, 7).Value = p.ArrTugsIn;
                ws2.Cell(row, 8).Value = p.ArrDraftFwd;
                ws2.Cell(row, 9).Value = p.ArrDraftAft;
                ws2.Cell(row, 10).Value = p.ArrDraftMean;
                ws2.Cell(row, 11).Value = p.ArrFreshWater;
                ws2.Cell(row, 12).Value = p.ArrBallastWater;
                ws2.Cell(row, 13).Value = p.ArrRemarks;
                if (p.HasArrival) ws2.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#f0fdf4");
                row++;
            }
            ws2.Columns(1, arrHeaders.Length).AdjustToContents();

            // --- Sheet 3: Departure Reports ---
            var ws3 = wb.AddWorksheet("Departure Reports");
            var depHeaders = new[] { "Port", "Voy No", "Actual ETD", "Complete Cargo Ops", "Pilot On Board",
                "Unberth FAOP", "Tugs Out", "Draft Fwd(m)", "Draft Aft(m)", "Draft Mean(m)",
                "Fresh Water", "Ballast Water", "Remarks" };
            for (int i = 0; i < depHeaders.Length; i++)
                ws3.Cell(1, i + 1).Value = depHeaders[i];
            ws3.Range(1, 1, 1, depHeaders.Length).Style.Font.Bold = true;
            ws3.Range(1, 1, 1, depHeaders.Length).Style.Fill.BackgroundColor = XLColor.FromHtml("#7c2d12");
            ws3.Range(1, 1, 1, depHeaders.Length).Style.Font.FontColor = XLColor.White;
            row = 2;
            foreach (var p in ports)
            {
                ws3.Cell(row, 1).Value = p.PortCode;
                ws3.Cell(row, 2).Value = p.VoyNo;
                ws3.Cell(row, 3).Value = Fmt(p.ActualETD);
                ws3.Cell(row, 4).Value = Fmt(p.DepCompleteCargoOp);
                ws3.Cell(row, 5).Value = Fmt(p.DepPilotOnBoard);
                ws3.Cell(row, 6).Value = Fmt(p.DepUnberthFAOP);
                ws3.Cell(row, 7).Value = p.DepTugsOut;
                ws3.Cell(row, 8).Value = p.DepDraftFwd;
                ws3.Cell(row, 9).Value = p.DepDraftAft;
                ws3.Cell(row, 10).Value = p.DepDraftMean;
                ws3.Cell(row, 11).Value = p.DepFreshWater;
                ws3.Cell(row, 12).Value = p.DepBallastWater;
                ws3.Cell(row, 13).Value = p.DepRemarks;
                if (p.HasDeparture) ws3.Row(row).Style.Fill.BackgroundColor = XLColor.FromHtml("#fffbeb");
                row++;
            }
            ws3.Columns(1, depHeaders.Length).AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var fileName = $"VoyagePort_{voyage.VesselName?.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportArrival(Guid id, CancellationToken ct)
        {
            // id = VoyagePortId
            var arrival = await _voyageService.GetArrivalByVoyagePortIdAsync(id, ct);
            if (arrival == null) return NotFound();

            var allVoyages = await _voyageService.GetAllAsync(ct);
            VoyagePortListItemDto? portItem = null;
            VoyageListItemDto? parentVoyage = null;
            foreach (var v in allVoyages)
            {
                var ports = await _voyageService.GetPortsByVoyageIdAsync(v.Id, ct);
                portItem = ports.FirstOrDefault(p => p.Id == id);
                if (portItem != null) { parentVoyage = v; break; }
            }

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Arrival Report");

            // Match exact template layout: 4 columns (A-D)
            ws.Column(1).Width = 42;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 30;
            ws.Column(4).Width = 16;

            var labelFont = new Action<IXLCell>(c => { c.Style.Font.FontSize = 11; });
            var valFont = new Action<IXLCell>(c => { c.Style.Font.Bold = true; c.Style.Font.FontSize = 12; });

            // Row 1: Title (merged A1:D1, centered)
            ws.Cell(1, 1).Value = "VESSEL ARRIVAL REPORT";
            ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(1, 1, 1, 4).Merge();

            // Row 2: Port
            ws.Cell(2, 1).Value = "Port :"; labelFont(ws.Cell(2, 1));
            ws.Cell(2, 2).Value = portItem?.PortCode ?? ""; valFont(ws.Cell(2, 2));
            ws.Range(2, 2, 2, 4).Merge();

            // Row 3: Vessel Name
            ws.Cell(3, 1).Value = "Vessel Name :"; labelFont(ws.Cell(3, 1));
            ws.Cell(3, 2).Value = parentVoyage?.VesselName ?? ""; valFont(ws.Cell(3, 2));
            ws.Range(3, 2, 3, 3).Merge();

            // Row 4: Terminal
            ws.Cell(4, 1).Value = portItem?.TerminalCode ?? ""; labelFont(ws.Cell(4, 1));

            // Row 5: Voyages
            ws.Cell(5, 1).Value = "Inbound Voyage :"; labelFont(ws.Cell(5, 1));
            ws.Cell(5, 2).Value = arrival.InboundVoyage ?? ""; valFont(ws.Cell(5, 2));
            ws.Cell(5, 3).Value = "Outbound Voyage :"; labelFont(ws.Cell(5, 3));
            ws.Cell(5, 4).Value = arrival.OutboundVoyage ?? ""; valFont(ws.Cell(5, 4));

            // Row 6: Last Port
            ws.Cell(6, 1).Value = "Last Port :"; labelFont(ws.Cell(6, 1));
            ws.Cell(6, 2).Value = arrival.LastPortCode ?? ""; valFont(ws.Cell(6, 2));
            ws.Range(6, 2, 6, 4).Merge();

            // Row 7: Arrival Time (End of Passage) = Actual ETA
            ws.Cell(7, 1).Value = "Arrival Time (End of Passage )"; labelFont(ws.Cell(7, 1));
            ws.Cell(7, 2).Value = arrival.ActualETA?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(7, 2));
            ws.Cell(7, 3).Value = "End of Passage (hh:mm)"; labelFont(ws.Cell(7, 3));
            ws.Cell(7, 4).Value = arrival.ActualETA?.ToString("HHmm") ?? ""; valFont(ws.Cell(7, 4));

            // Row 8: Pilot On Board
            ws.Cell(8, 1).Value = "Pilot On Board  (dd/mm/yy)"; labelFont(ws.Cell(8, 1));
            ws.Cell(8, 2).Value = arrival.PilotOnBoard?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(8, 2));
            ws.Cell(8, 3).Value = "Pilot On Board  (hh:mm) "; labelFont(ws.Cell(8, 3));
            ws.Cell(8, 4).Value = arrival.PilotOnBoard?.ToString("HHmm") ?? ""; valFont(ws.Cell(8, 4));

            // Row 9: Arrival Berth = Actual ETB
            ws.Cell(9, 1).Value = "Arrival Berth  (dd/mm/yy)"; labelFont(ws.Cell(9, 1));
            ws.Cell(9, 2).Value = arrival.ActualETB?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(9, 2));
            ws.Cell(9, 3).Value = "Arrival Berth  (hh:mm)"; labelFont(ws.Cell(9, 3));
            ws.Cell(9, 4).Value = arrival.ActualETB?.ToString("HHmm") ?? ""; valFont(ws.Cell(9, 4));

            // Row 10: Commenced Cargo Operation
            ws.Cell(10, 1).Value = "Commenced Cargo Operation   (dd/mm/yy)"; labelFont(ws.Cell(10, 1));
            ws.Cell(10, 2).Value = arrival.CommencedCargoOperation?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(10, 2));
            ws.Cell(10, 3).Value = "Comm.Cgo.Ops.    (hh:mm)"; labelFont(ws.Cell(10, 3));
            ws.Cell(10, 4).Value = arrival.CommencedCargoOperation?.ToString("HHmm") ?? ""; valFont(ws.Cell(10, 4));

            // Row 11: Estimated Time of Departure
            ws.Cell(11, 1).Value = "Estimated Time of Departure  (dd/mm/yy) "; labelFont(ws.Cell(11, 1));
            ws.Cell(11, 2).Value = arrival.EstimatedETD?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(11, 2));
            ws.Cell(11, 3).Value = "Est.Time of Sailing   (hh:mm) "; labelFont(ws.Cell(11, 3));
            ws.Cell(11, 4).Value = arrival.EstimatedETD?.ToString("HHmm") ?? ""; valFont(ws.Cell(11, 4));

            // Row 12: Next Port
            ws.Cell(12, 1).Value = "Next Port : "; ws.Cell(12, 1).Style.Font.Bold = true; ws.Cell(12, 1).Style.Font.FontSize = 14;
            ws.Cell(12, 2).Value = arrival.NextPortCode ?? ""; valFont(ws.Cell(12, 2));
            ws.Range(12, 2, 12, 4).Merge();

            // Row 13: Tugs In
            ws.Cell(13, 1).Value = "Tugs In"; labelFont(ws.Cell(13, 1));
            ws.Cell(13, 2).Value = arrival.TugsIn ?? 0; valFont(ws.Cell(13, 2));

            // Row 14-16: Draft
            ws.Cell(14, 1).Value = "Arrival Draft (Fwd) in meters "; labelFont(ws.Cell(14, 1));
            ws.Cell(14, 2).Value = arrival.ArrivalDraftFwdMtr?.ToString("0.##") ?? ""; valFont(ws.Cell(14, 2));

            ws.Cell(15, 1).Value = "Arrival Draft (Aft) in meters "; labelFont(ws.Cell(15, 1));
            ws.Cell(15, 2).Value = arrival.ArrivalDraftAftMtr?.ToString("0.##") ?? ""; valFont(ws.Cell(15, 2));

            ws.Cell(16, 1).Value = "Arrival Draft (Mean) in meters "; labelFont(ws.Cell(16, 1));
            ws.Cell(16, 2).Value = arrival.ArrivalDraftMeanMtr?.ToString("0.##") ?? ""; valFont(ws.Cell(16, 2));

            // Row 17-18: Bunkers
            ws.Cell(17, 1).Value = "Bunkers On Arrival"; labelFont(ws.Cell(17, 1));
            ws.Cell(17, 2).Value = "Fresh Water"; labelFont(ws.Cell(17, 2));
            ws.Cell(17, 3).Value = arrival.FreshWater?.ToString("0.#") ?? ""; valFont(ws.Cell(17, 3));
            ws.Range(17, 1, 18, 1).Merge();
            ws.Cell(17, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(17, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell(18, 2).Value = "Ballast Water"; labelFont(ws.Cell(18, 2));
            ws.Cell(18, 3).Value = arrival.BallastWater?.ToString("0.#") ?? ""; valFont(ws.Cell(18, 3));

            // Row 19: Remarks (merge B and C for value)
            ws.Cell(19, 1).Value = "REMARKS:"; ws.Cell(19, 1).Style.Font.Bold = true; ws.Cell(19, 1).Style.Font.FontSize = 12;
            ws.Cell(19, 2).Value = arrival.Remarks ?? ""; valFont(ws.Cell(19, 2));
            ws.Range(19, 2, 19, 3).Merge();
            ws.Cell(19, 2).Style.Alignment.WrapText = true;

            // Add borders to all used cells
            var usedRange = ws.Range(1, 1, 19, 4);
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var fileName = $"ArrivalReport_{portItem?.PortCode}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [HttpGet]
        public async Task<IActionResult> ExportDeparture(Guid id, CancellationToken ct)
        {
            // id = VoyagePortId
            var departure = await _voyageService.GetDepartureByVoyagePortIdAsync(id, ct);
            if (departure == null) return NotFound();

            var allVoyages = await _voyageService.GetAllAsync(ct);
            VoyagePortListItemDto? portItem = null;
            VoyageListItemDto? parentVoyage = null;
            foreach (var v in allVoyages)
            {
                var ports = await _voyageService.GetPortsByVoyageIdAsync(v.Id, ct);
                portItem = ports.FirstOrDefault(p => p.Id == id);
                if (portItem != null) { parentVoyage = v; break; }
            }

            using var wb = new XLWorkbook();
            var ws = wb.AddWorksheet("Departure Report");

            // Match exact template layout: 4 columns (A-D)
            ws.Column(1).Width = 40;
            ws.Column(2).Width = 20;
            ws.Column(3).Width = 28;
            ws.Column(4).Width = 16;

            var labelFont = new Action<IXLCell>(c => { c.Style.Font.FontSize = 10; });
            var valFont = new Action<IXLCell>(c => { c.Style.Font.Bold = true; c.Style.Font.FontSize = 12; });

            // Row 1: Title (merged A1:D1, centered)
            ws.Cell(1, 1).Value = "VESSEL DEPARTURE REPORT";
            ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 14;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Range(1, 1, 1, 4).Merge();

            // Row 2: Port
            ws.Cell(2, 1).Value = "Port :"; labelFont(ws.Cell(2, 1));
            ws.Cell(2, 2).Value = portItem?.PortCode ?? ""; valFont(ws.Cell(2, 2));
            ws.Range(2, 2, 2, 4).Merge();

            // Row 3: Terminal
            ws.Cell(3, 1).Value = "TERMINAL"; labelFont(ws.Cell(3, 1));
            ws.Cell(3, 2).Value = portItem?.TerminalCode ?? ""; valFont(ws.Cell(3, 2));

            // Row 4: Vessel Name
            ws.Cell(4, 1).Value = "Vessel Name :"; labelFont(ws.Cell(4, 1));
            ws.Cell(4, 2).Value = parentVoyage?.VesselName ?? ""; valFont(ws.Cell(4, 2));
            ws.Range(4, 2, 4, 3).Merge();

            // Row 5: Voyages
            ws.Cell(5, 1).Value = "Inbound Voyage :"; labelFont(ws.Cell(5, 1));
            ws.Cell(5, 2).Value = departure.InboundVoyage ?? ""; valFont(ws.Cell(5, 2));
            ws.Cell(5, 3).Value = "Outbound Voyage :"; labelFont(ws.Cell(5, 3));
            ws.Cell(5, 4).Value = departure.OutboundVoyage ?? ""; valFont(ws.Cell(5, 4));

            // Row 6: Complete Cargo Operation
            ws.Cell(6, 1).Value = "Complete Cargo Operation (dd/mm/yy)"; labelFont(ws.Cell(6, 1));
            ws.Cell(6, 2).Value = departure.CompleteCargoOperation?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(6, 2));
            ws.Cell(6, 3).Value = "Comp. Cgo. Ops. (hh:mm)"; labelFont(ws.Cell(6, 3));
            ws.Cell(6, 4).Value = departure.CompleteCargoOperation?.ToString("HHmm") ?? ""; valFont(ws.Cell(6, 4));

            // Row 7: Pilot On Board
            ws.Cell(7, 1).Value = "Pilot On Board (dd/mm/yy)"; labelFont(ws.Cell(7, 1));
            ws.Cell(7, 2).Value = departure.PilotOnBoard?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(7, 2));
            ws.Cell(7, 3).Value = "Pilot On Board (hh:mm)"; labelFont(ws.Cell(7, 3));
            ws.Cell(7, 4).Value = departure.PilotOnBoard?.ToString("HHmm") ?? ""; valFont(ws.Cell(7, 4));

            // Row 8: Unberth FAOP
            ws.Cell(8, 1).Value = "Unberth- FAOP (dd/mm/yy) "; labelFont(ws.Cell(8, 1));
            ws.Cell(8, 2).Value = departure.UnberthFAOP?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(8, 2));
            ws.Cell(8, 3).Value = "Unberth - FAOP (hh:mm) "; labelFont(ws.Cell(8, 3));
            ws.Cell(8, 4).Value = departure.UnberthFAOP?.ToString("HHmm") ?? ""; valFont(ws.Cell(8, 4));

            // Row 9: Departure (BOSP) = Actual ETD
            ws.Cell(9, 1).Value = "Departure (BOSP)"; labelFont(ws.Cell(9, 1));
            ws.Cell(9, 2).Value = departure.ActualETD?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(9, 2));
            ws.Cell(9, 3).Value = "Departure (BOSP)"; labelFont(ws.Cell(9, 3));
            ws.Cell(9, 4).Value = departure.ActualETD?.ToString("HHmm") ?? ""; valFont(ws.Cell(9, 4));

            // Row 10: Next Port
            ws.Cell(10, 1).Value = " Next Port :"; labelFont(ws.Cell(10, 1));
            ws.Cell(10, 2).Value = departure.NextPortCode ?? ""; valFont(ws.Cell(10, 2));
            ws.Range(10, 2, 10, 3).Merge();

            // Row 11: ETA Next Port
            ws.Cell(11, 1).Value = "E T A Next Port (dd/mm/yy) "; labelFont(ws.Cell(11, 1));
            ws.Cell(11, 2).Value = departure.ETANextPort?.ToString("dd.MM.yyyy") ?? ""; valFont(ws.Cell(11, 2));
            ws.Cell(11, 3).Value = "E T A Next Port (hh:mm) "; labelFont(ws.Cell(11, 3));
            ws.Cell(11, 4).Value = departure.ETANextPort?.ToString("HHmm") ?? ""; valFont(ws.Cell(11, 4));

            // Row 12: Tugs Out
            ws.Cell(12, 1).Value = "Tugs Out"; labelFont(ws.Cell(12, 1));
            ws.Cell(12, 2).Value = departure.TugsOut ?? 0; valFont(ws.Cell(12, 2));

            // Row 13-15: Draft
            ws.Cell(13, 1).Value = "Dep. Draft (Fwd) in meters "; labelFont(ws.Cell(13, 1));
            ws.Cell(13, 2).Value = departure.DepDraftFwdMtr?.ToString("0.##") ?? ""; valFont(ws.Cell(13, 2));

            ws.Cell(14, 1).Value = "Dep. Draft (Aft) in meters "; labelFont(ws.Cell(14, 1));
            ws.Cell(14, 2).Value = departure.DepDraftAftMtr?.ToString("0.##") ?? ""; valFont(ws.Cell(14, 2));

            ws.Cell(15, 1).Value = "Dep. Draft (Mean) in meters "; labelFont(ws.Cell(15, 1));
            ws.Cell(15, 2).Value = departure.DepDraftMeanMtr?.ToString("0.##") ?? ""; valFont(ws.Cell(15, 2));

            // Row 16-17: Bunkers
            ws.Cell(16, 1).Value = "Bunkers on Departure"; labelFont(ws.Cell(16, 1));
            ws.Cell(16, 2).Value = "Fresh Water"; labelFont(ws.Cell(16, 2));
            ws.Cell(16, 3).Value = departure.FreshWater?.ToString("0.#") ?? ""; valFont(ws.Cell(16, 3));
            ws.Range(16, 1, 17, 1).Merge();
            ws.Cell(16, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(16, 1).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell(17, 2).Value = "Ballast Water"; labelFont(ws.Cell(17, 2));
            ws.Cell(17, 3).Value = departure.BallastWater?.ToString("0.#") ?? ""; valFont(ws.Cell(17, 3));

            // Row 18: Remarks (merge B and C for value)
            ws.Cell(18, 1).Value = "Remarks :"; ws.Cell(18, 1).Style.Font.Bold = true; ws.Cell(18, 1).Style.Font.FontSize = 11;
            ws.Cell(18, 2).Value = departure.Remarks ?? ""; valFont(ws.Cell(18, 2));
            ws.Range(18, 2, 18, 3).Merge();
            ws.Cell(18, 2).Style.Alignment.WrapText = true;

            // Add borders to all used cells
            var usedRange = ws.Range(1, 1, 18, 4);
            usedRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            usedRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            var fileName = $"DepartureReport_{portItem?.PortCode}_{DateTime.Now:yyyyMMdd}.xlsx";
            return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
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

            // Distance lookup — used to auto-fill voyage port distance from Distance Master
            var distances = await _distanceMasterService.GetAllAsync(ct);
            ViewBag.Distances = distances.Select(d => new
            {
                FromPortId = d.FromPortId.ToString(),
                ToPortId = d.ToPortId.ToString(),
                Distance = d.Distance
            }).ToList();
        }
    }
}
