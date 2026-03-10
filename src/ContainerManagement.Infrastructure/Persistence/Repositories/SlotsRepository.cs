using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Slots;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class SlotsRepository : ISlotsRepository
    {
        private readonly AppDbContext _context;

        public SlotsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<SlotMaster>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<SlotEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.SlotName)
                .Select(x => new SlotMaster
                {
                    Id = x.Id,
                    SlotName = x.SlotName,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<SlotMaster?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<SlotEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new SlotMaster
                {
                    Id = x.Id,
                    SlotName = x.SlotName,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(string slotName, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<SlotEntity>()
                .Where(x => x.SlotName == slotName && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(SlotMaster slot, CancellationToken ct = default)
        {
            var entity = new SlotEntity
            {
                Id = slot.Id == Guid.Empty ? Guid.NewGuid() : slot.Id,
                SlotName = slot.SlotName,
                IsDeleted = false,
                CreatedOn = slot.CreatedOn == default ? DateTime.UtcNow : slot.CreatedOn,
                ModifiedOn = slot.ModifiedOn == default ? DateTime.UtcNow : slot.ModifiedOn,
                CreatedBy = slot.CreatedBy,
                ModifiedBy = slot.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            slot.Id = entity.Id;
        }

        public async Task UpdateAsync(SlotMaster slot, CancellationToken ct = default)
        {
            var entity = await _context.Set<SlotEntity>()
                .FirstOrDefaultAsync(x => x.Id == slot.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Slot not found.");

            entity.SlotName = slot.SlotName;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = slot.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<SlotEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Slot not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}
