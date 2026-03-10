using ContainerManagement.Domain.Slots;

namespace ContainerManagement.Application.Abstractions
{
    public interface ISlotsRepository
    {
        Task<List<SlotMaster>> GetAllAsync(CancellationToken ct = default);
        Task<SlotMaster?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string slotName, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(SlotMaster slot, CancellationToken ct = default);
        Task UpdateAsync(SlotMaster slot, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}
