using ContainerManagement.Application.Dtos.Jobs;
using ContainerManagement.Application.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ContainerManagement.Web.Controllers
{
    public class JobsController : Controller
    {
        private readonly JobService _jobService;
        private readonly IWebHostEnvironment _env;

        public JobsController(JobService jobService, IWebHostEnvironment env)
        {
            _jobService = jobService;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var list = await _jobService.GetAllAsync(ct);
            return View(list);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] JobCreateDto dto, CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Validation failed." });

                if (!TryGetUserId(out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                dto.CreatedBy = userId;
                var result = await _jobService.CreateAsync(dto, ct);
                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromBody] JobUpdateDto dto, CancellationToken ct)
        {
            try
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { success = false, message = "Validation failed." });

                if (!TryGetUserId(out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                dto.ModifiedBy = userId;
                var ok = await _jobService.UpdateAsync(dto, ct);
                return ok
                    ? Ok(new { success = true })
                    : NotFound(new { success = false, message = "Job not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateStatusRequest req, CancellationToken ct)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                var ok = await _jobService.UpdateStatusAsync(req.Id, req.Status, userId, ct);
                return ok
                    ? Ok(new { success = true })
                    : NotFound(new { success = false, message = "Job not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete([FromBody] DeleteRequest req, CancellationToken ct)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                var ok = await _jobService.DeleteAsync(req.Id, userId, ct);
                return ok
                    ? Ok(new { success = true })
                    : NotFound(new { success = false, message = "Job not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        // ── Attachments ──

        [HttpGet]
        public async Task<IActionResult> GetAttachments(Guid jobId, CancellationToken ct)
        {
            try
            {
                var atts = await _jobService.GetAttachmentsAsync(jobId, ct);
                return Ok(new { success = true, data = atts });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadAttachment(Guid jobId, IFormFile file, bool isScreenshot, CancellationToken ct)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "No file provided." });

                var ext = Path.GetExtension(file.FileName);
                var storedName = $"{Guid.NewGuid()}{ext}";
                var dir = Path.Combine(_env.WebRootPath, "uploads", "jobs", jobId.ToString());
                Directory.CreateDirectory(dir);

                var filePath = Path.Combine(dir, storedName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream, ct);
                }

                var result = await _jobService.AddAttachmentAsync(
                    jobId, file.FileName, storedName, file.ContentType, file.Length, isScreenshot, userId, ct);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UploadScreenshot([FromBody] ScreenshotRequest req, CancellationToken ct)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                if (string.IsNullOrEmpty(req.ImageData))
                    return BadRequest(new { success = false, message = "No image data." });

                // Parse base64 data URL: "data:image/png;base64,iVBOR..."
                var parts = req.ImageData.Split(',');
                if (parts.Length != 2)
                    return BadRequest(new { success = false, message = "Invalid image format." });

                var meta = parts[0]; // "data:image/png;base64"
                var base64 = parts[1];
                var bytes = Convert.FromBase64String(base64);

                var contentType = meta.Replace("data:", "").Replace(";base64", "");
                var ext = contentType switch
                {
                    "image/png" => ".png",
                    "image/jpeg" => ".jpg",
                    "image/gif" => ".gif",
                    "image/webp" => ".webp",
                    _ => ".png"
                };

                var storedName = $"{Guid.NewGuid()}{ext}";
                var fileName = $"screenshot-{DateTime.UtcNow:yyyyMMdd-HHmmss}{ext}";
                var dir = Path.Combine(_env.WebRootPath, "uploads", "jobs", req.JobId.ToString());
                Directory.CreateDirectory(dir);

                var filePath = Path.Combine(dir, storedName);
                await System.IO.File.WriteAllBytesAsync(filePath, bytes, ct);

                var result = await _jobService.AddAttachmentAsync(
                    req.JobId, fileName, storedName, contentType, bytes.Length, true, userId, ct);

                return Ok(new { success = true, data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAttachment([FromBody] DeleteRequest req, CancellationToken ct)
        {
            try
            {
                if (!TryGetUserId(out var userId))
                    return Unauthorized(new { success = false, message = "Invalid session." });

                var att = await _jobService.GetAttachmentByIdAsync(req.Id, ct);
                if (att == null)
                    return NotFound(new { success = false, message = "Attachment not found." });

                // Delete file from disk
                var filePath = Path.Combine(_env.WebRootPath, "uploads", "jobs", att.JobId.ToString(), att.StoredFileName);
                if (System.IO.File.Exists(filePath))
                    System.IO.File.Delete(filePath);

                var ok = await _jobService.DeleteAttachmentAsync(req.Id, userId, ct);
                return ok
                    ? Ok(new { success = true })
                    : NotFound(new { success = false, message = "Attachment not found." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private bool TryGetUserId(out Guid userId)
        {
            userId = Guid.Empty;
            var str = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(str, out userId);
        }

        public class UpdateStatusRequest
        {
            public Guid Id { get; set; }
            public int Status { get; set; }
        }

        public class DeleteRequest
        {
            public Guid Id { get; set; }
        }

        public class ScreenshotRequest
        {
            public Guid JobId { get; set; }
            public string ImageData { get; set; } = string.Empty;
        }
    }
}
