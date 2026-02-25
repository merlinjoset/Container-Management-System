using ContainerManagement.Domain.Vendors;

namespace ContainerManagement.Application.Abstractions
{
    public interface IVendorsRepository
    {
        Task<List<Vendor>> GetAllAsync(CancellationToken ct = default);
        Task<Vendor?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<bool> ExistsAsync(string vendorCode, Guid? excludeId = null, CancellationToken ct = default);

        Task AddAsync(Vendor vendor, CancellationToken ct = default);
        Task UpdateAsync(Vendor vendor, CancellationToken ct = default);
        Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default);
    }
}

