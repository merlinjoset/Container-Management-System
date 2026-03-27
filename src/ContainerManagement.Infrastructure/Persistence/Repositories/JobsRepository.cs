using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Jobs;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class JobsRepository : IJobsRepository
    {
        private readonly AppDbContext _context;

        public JobsRepository(AppDbContext context) => _context = context;

        public async Task<List<Job>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<JobEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderByDescending(x => x.CreatedOn)
                .Select(x => new Job
                {
                    Id = x.Id,
                    Title = x.Title,
                    Description = x.Description,
                    Status = (JobStatus)x.Status,
                    Tag = x.Tag,
                    TagColor = x.TagColor,
                    CompletedDate = x.CompletedDate,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy,
                    IsDeleted = x.IsDeleted
                })
                .ToListAsync(ct);
        }

        public async Task<Job?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            var x = await _context.Set<JobEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);

            if (x == null) return null;

            return new Job
            {
                Id = x.Id,
                Title = x.Title,
                Description = x.Description,
                Status = (JobStatus)x.Status,
                Tag = x.Tag,
                TagColor = x.TagColor,
                CompletedDate = x.CompletedDate,
                CreatedOn = x.CreatedOn,
                ModifiedOn = x.ModifiedOn,
                CreatedBy = x.CreatedBy,
                ModifiedBy = x.ModifiedBy,
                IsDeleted = x.IsDeleted
            };
        }

        public async Task AddAsync(Job job, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var entity = new JobEntity
            {
                Id = job.Id == Guid.Empty ? Guid.NewGuid() : job.Id,
                Title = job.Title,
                Description = job.Description,
                Status = (int)job.Status,
                Tag = job.Tag,
                TagColor = job.TagColor,
                CompletedDate = job.CompletedDate,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = job.CreatedBy,
                ModifiedBy = job.CreatedBy,
                IsDeleted = false
            };

            _context.Set<JobEntity>().Add(entity);
            await _context.SaveChangesAsync(ct);
        }

        public async Task UpdateAsync(Job job, CancellationToken ct = default)
        {
            var entity = await _context.Set<JobEntity>()
                .FirstOrDefaultAsync(e => e.Id == job.Id && !e.IsDeleted, ct);

            if (entity == null) return;

            entity.Title = job.Title;
            entity.Description = job.Description;
            entity.Status = (int)job.Status;
            entity.Tag = job.Tag;
            entity.TagColor = job.TagColor;
            entity.CompletedDate = job.CompletedDate;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = job.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<JobEntity>()
                .FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, ct);

            if (entity == null) return;

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}
