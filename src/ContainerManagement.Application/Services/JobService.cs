using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Jobs;
using ContainerManagement.Domain.Jobs;

namespace ContainerManagement.Application.Services
{
    public class JobService
    {
        private readonly IJobsRepository _repo;
        private readonly IJobAttachmentsRepository _attachRepo;

        public JobService(IJobsRepository repo, IJobAttachmentsRepository attachRepo)
        {
            _repo = repo;
            _attachRepo = attachRepo;
        }

        public async Task<List<JobListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var jobs = await _repo.GetAllAsync(ct);
            var dtos = new List<JobListItemDto>();

            foreach (var j in jobs)
            {
                var atts = await _attachRepo.GetByJobIdAsync(j.Id, ct);
                dtos.Add(new JobListItemDto
                {
                    Id = j.Id,
                    Title = j.Title,
                    Description = j.Description,
                    Status = (int)j.Status,
                    Tag = j.Tag,
                    TagColor = j.TagColor,
                    CompletedDate = j.CompletedDate,
                    CreatedOn = j.CreatedOn,
                    ModifiedOn = j.ModifiedOn,
                    Attachments = atts.Select(a => MapAttachmentDto(a)).ToList()
                });
            }

            return dtos;
        }

        public async Task<JobListItemDto?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var j = await _repo.GetByIdAsync(id, ct);
            if (j == null) return null;

            var atts = await _attachRepo.GetByJobIdAsync(j.Id, ct);
            return new JobListItemDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                Status = (int)j.Status,
                Tag = j.Tag,
                TagColor = j.TagColor,
                CompletedDate = j.CompletedDate,
                CreatedOn = j.CreatedOn,
                ModifiedOn = j.ModifiedOn,
                Attachments = atts.Select(a => MapAttachmentDto(a)).ToList()
            };
        }

        public async Task<JobListItemDto> CreateAsync(JobCreateDto dto, CancellationToken ct = default)
        {
            var job = new Job
            {
                Id = Guid.NewGuid(),
                Title = dto.Title,
                Description = dto.Description,
                Status = (JobStatus)dto.Status,
                Tag = dto.Tag,
                TagColor = dto.TagColor,
                CreatedBy = dto.CreatedBy,
                CompletedDate = (JobStatus)dto.Status == JobStatus.Done ? DateTime.UtcNow : null
            };

            await _repo.AddAsync(job, ct);

            return new JobListItemDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                Status = (int)job.Status,
                Tag = job.Tag,
                TagColor = job.TagColor,
                CompletedDate = job.CompletedDate,
                CreatedOn = DateTime.UtcNow,
                ModifiedOn = DateTime.UtcNow
            };
        }

        public async Task<bool> UpdateAsync(JobUpdateDto dto, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(dto.Id, ct);
            if (existing == null) return false;

            var wasNotDone = existing.Status != JobStatus.Done;
            var isNowDone = (JobStatus)dto.Status == JobStatus.Done;

            existing.Title = dto.Title;
            existing.Description = dto.Description;
            existing.Status = (JobStatus)dto.Status;
            existing.Tag = dto.Tag;
            existing.TagColor = dto.TagColor;
            existing.ModifiedBy = dto.ModifiedBy;

            if (wasNotDone && isNowDone)
                existing.CompletedDate = DateTime.UtcNow;
            else if (!isNowDone)
                existing.CompletedDate = null;

            await _repo.UpdateAsync(existing, ct);
            return true;
        }

        public async Task<bool> UpdateStatusAsync(Guid id, int status, Guid modifiedBy, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing == null) return false;

            var wasNotDone = existing.Status != JobStatus.Done;
            var isNowDone = (JobStatus)status == JobStatus.Done;

            existing.Status = (JobStatus)status;
            existing.ModifiedBy = modifiedBy;

            if (wasNotDone && isNowDone)
                existing.CompletedDate = DateTime.UtcNow;
            else if (!isNowDone)
                existing.CompletedDate = null;

            await _repo.UpdateAsync(existing, ct);
            return true;
        }

        public async Task<bool> DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var existing = await _repo.GetByIdAsync(id, ct);
            if (existing == null) return false;

            await _repo.SoftDeleteAsync(id, modifiedBy, ct);
            return true;
        }

        // ── Attachments ──

        public async Task<List<JobAttachmentDto>> GetAttachmentsAsync(Guid jobId, CancellationToken ct = default)
        {
            var atts = await _attachRepo.GetByJobIdAsync(jobId, ct);
            return atts.Select(a => MapAttachmentDto(a)).ToList();
        }

        public async Task<JobAttachmentDto> AddAttachmentAsync(Guid jobId, string fileName, string storedFileName,
            string contentType, long fileSize, bool isScreenshot, Guid createdBy, CancellationToken ct = default)
        {
            var att = new JobAttachment
            {
                Id = Guid.NewGuid(),
                JobId = jobId,
                FileName = fileName,
                StoredFileName = storedFileName,
                ContentType = contentType,
                FileSize = fileSize,
                IsScreenshot = isScreenshot,
                CreatedBy = createdBy
            };

            await _attachRepo.AddAsync(att, ct);

            return new JobAttachmentDto
            {
                Id = att.Id,
                JobId = att.JobId,
                FileName = att.FileName,
                ContentType = att.ContentType,
                FileSize = att.FileSize,
                IsScreenshot = att.IsScreenshot,
                CreatedOn = DateTime.UtcNow,
                Url = $"/uploads/jobs/{jobId}/{storedFileName}"
            };
        }

        public async Task<JobAttachment?> GetAttachmentByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _attachRepo.GetByIdAsync(id, ct);
        }

        public async Task<bool> DeleteAttachmentAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var existing = await _attachRepo.GetByIdAsync(id, ct);
            if (existing == null) return false;

            await _attachRepo.SoftDeleteAsync(id, modifiedBy, ct);
            return true;
        }

        private static JobAttachmentDto MapAttachmentDto(JobAttachment a) => new()
        {
            Id = a.Id,
            JobId = a.JobId,
            FileName = a.FileName,
            ContentType = a.ContentType,
            FileSize = a.FileSize,
            IsScreenshot = a.IsScreenshot,
            CreatedOn = a.CreatedOn,
            Url = $"/uploads/jobs/{a.JobId}/{a.StoredFileName}"
        };
    }
}
