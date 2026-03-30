using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Jobs;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class JobAttachmentsRepository : IJobAttachmentsRepository
    {
        private readonly AppDbContext _context;

        public JobAttachmentsRepository(AppDbContext context) => _context = context;

        public async Task<List<JobAttachment>> GetByJobIdAsync(Guid jobId, CancellationToken ct = default)
        {
            return await _context.Set<JobAttachmentEntity>()
                .AsNoTracking()
                .Where(x => x.JobId == jobId && !x.IsDeleted)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new JobAttachment
                {
                    Id = x.Id,
                    JobId = x.JobId,
                    FileName = x.FileName,
                    StoredFileName = x.StoredFileName,
                    ContentType = x.ContentType,
                    FileSize = x.FileSize,
                    IsScreenshot = x.IsScreenshot,
                    // FileData intentionally excluded for list queries
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy,
                    IsDeleted = x.IsDeleted
                })
                .ToListAsync(ct);
        }

        public async Task<JobAttachment?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var x = await _context.Set<JobAttachmentEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);

            return x == null ? null : MapToDomain(x);
        }

        public async Task AddAsync(JobAttachment att, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var entity = new JobAttachmentEntity
            {
                Id = att.Id == Guid.Empty ? Guid.NewGuid() : att.Id,
                JobId = att.JobId,
                FileName = att.FileName,
                StoredFileName = att.StoredFileName,
                ContentType = att.ContentType,
                FileSize = att.FileSize,
                IsScreenshot = att.IsScreenshot,
                FileData = att.FileData,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = att.CreatedBy,
                ModifiedBy = att.CreatedBy,
                IsDeleted = false
            };

            _context.Set<JobAttachmentEntity>().Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<JobAttachmentEntity>()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);

            if (entity == null) return;

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        private static JobAttachment MapToDomain(JobAttachmentEntity x) => new()
        {
            Id = x.Id,
            JobId = x.JobId,
            FileName = x.FileName,
            StoredFileName = x.StoredFileName,
            ContentType = x.ContentType,
            FileSize = x.FileSize,
            IsScreenshot = x.IsScreenshot,
            FileData = x.FileData,
            CreatedOn = x.CreatedOn,
            ModifiedOn = x.ModifiedOn,
            CreatedBy = x.CreatedBy,
            ModifiedBy = x.ModifiedBy,
            IsDeleted = x.IsDeleted
        };
    }
}
