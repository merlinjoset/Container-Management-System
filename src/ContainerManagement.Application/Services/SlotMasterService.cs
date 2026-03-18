using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Slots;
using ContainerManagement.Domain.Slots;

namespace ContainerManagement.Application.Services
{
    public class SlotMasterService
    {
        private readonly ISlotsRepository _repository;

        public SlotMasterService(ISlotsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<SlotListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var slots = await _repository.GetAllAsync(ct);
            return slots.Select(x => new SlotListItemDto
            {
                Id = x.Id,
                SlotName = x.SlotName
            }).ToList();
        }

        public async Task<Guid> CreateAsync(SlotCreateDto dto, CancellationToken ct = default)
        {
            if (!string.IsNullOrWhiteSpace(dto.SlotName) && await _repository.ExistsAsync(dto.SlotName!, null, ct))
                throw new Exception("Slot name already exists.");

            var now = DateTime.UtcNow;

            var slot = new SlotMaster
            {
                Id = Guid.NewGuid(),
                SlotName = dto.SlotName,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _repository.AddAsync(slot, ct);
            return slot.Id;
        }

        public async Task UpdateAsync(SlotUpdateDto dto, CancellationToken ct = default)
        {
            var slot = await _repository.GetByIdAsync(dto.Id, ct);
            if (slot == null)
                throw new Exception("Slot not found.");

            if (!string.IsNullOrWhiteSpace(dto.SlotName) && await _repository.ExistsAsync(dto.SlotName!, dto.Id, ct))
                throw new Exception("Slot name already exists.");

            slot.SlotName = dto.SlotName;
            slot.ModifiedOn = DateTime.UtcNow;
            slot.ModifiedBy = dto.ModifiedBy;

            await _repository.UpdateAsync(slot, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _repository.SoftDeleteAsync(id, modifiedBy, ct);
        }

        public async Task<(int added, int updated, int skipped)> ImportAsync(
            IEnumerable<string?> slotNames, Guid userId, CancellationToken ct = default)
        {
            var existing = await _repository.GetAllAsync(ct);
            var byName = existing
                .Where(s => !string.IsNullOrWhiteSpace(s.SlotName))
                .ToDictionary(s => s.SlotName!.Trim().ToLowerInvariant(), s => s);

            int added = 0, skipped = 0;
            foreach (var raw in slotNames)
            {
                var name = (raw ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }

                var key = name.ToLowerInvariant();
                if (byName.ContainsKey(key))
                {
                    skipped++;
                }
                else
                {
                    var newSlot = new SlotMaster
                    {
                        Id = Guid.NewGuid(),
                        SlotName = name,
                        IsDeleted = false,
                        CreatedOn = DateTime.UtcNow,
                        ModifiedOn = DateTime.UtcNow,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    };
                    await _repository.AddAsync(newSlot, ct);
                    byName[key] = newSlot;
                    added++;
                }
            }
            return (added, 0, skipped);
        }
    }
}
