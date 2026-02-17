using ContainerManagement.Application.Dtos.Ports;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace ContainerManagement.Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PortsController : ControllerBase
    {
        private readonly PortService _portService;

        public PortsController(PortService portService)
        {
            _portService = portService;
        }

        // GET: api/ports
        [HttpGet]
        public async Task<ActionResult<List<PortListItemDto>>> GetAll(CancellationToken ct)
        {
            var result = await _portService.GetAllAsync(ct);
            return Ok(result);
        }

        // POST: api/ports
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] PortCreateDto dto, CancellationToken ct)
        {
            var id = await _portService.CreateAsync(dto, ct);
            return Ok(new { id });
        }

        // PUT: api/ports/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Edit(Guid id, [FromBody] PortUpdateDto dto, CancellationToken ct)
        {
            if (id != dto.Id)
                return BadRequest("Route id and body id do not match.");

            await _portService.UpdateAsync(dto, ct);
            return Ok("Port updated successfully");
        }

        // DELETE: api/ports/{id}?modifiedBy={guid}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid modifiedBy, CancellationToken ct)
        {
            await _portService.DeleteAsync(id, modifiedBy, ct);
            return Ok("Port deleted successfully");
        }
    }
}
