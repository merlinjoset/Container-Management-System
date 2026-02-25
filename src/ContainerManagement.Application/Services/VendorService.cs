using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Vendors;
using ContainerManagement.Domain.Vendors;

namespace ContainerManagement.Application.Services
{
    public class VendorService
    {
        private readonly IVendorsRepository _vendors;
        private readonly ICountriesRepository _countries;

        public VendorService(IVendorsRepository vendors, ICountriesRepository countries)
        {
            _vendors = vendors;
            _countries = countries;
        }

        public async Task<List<VendorListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var list = await _vendors.GetAllAsync(ct);
            var countries = await _countries.GetAllAsync(ct);
            var countryById = countries.ToDictionary(c => c.Id, c => c);

            return list.Select(v => new VendorListItemDto
            {
                Id = v.Id,
                VendorName = v.VendorName,
                VendorCode = v.VendorCode,
                CountryId = v.CountryId,
                CountryName = countryById.TryGetValue(v.CountryId, out var c) ? c.CountryName : null
            }).ToList();
        }

        public async Task<Guid> CreateAsync(VendorCreateDto dto, CancellationToken ct = default)
        {
            if (await _vendors.ExistsAsync(dto.VendorCode, null, ct))
                throw new Exception("Vendor code already exists.");

            var now = DateTime.UtcNow;
            var v = new Vendor
            {
                Id = Guid.NewGuid(),
                VendorName = dto.VendorName,
                VendorCode = dto.VendorCode,
                CountryId = dto.CountryId,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _vendors.AddAsync(v, ct);
            return v.Id;
        }

        public async Task UpdateAsync(VendorUpdateDto dto, CancellationToken ct = default)
        {
            var v = await _vendors.GetByIdAsync(dto.Id, ct);
            if (v == null) throw new Exception("Vendor not found.");

            if (await _vendors.ExistsAsync(dto.VendorCode, dto.Id, ct))
                throw new Exception("Vendor code already exists.");

            v.VendorName = dto.VendorName;
            v.VendorCode = dto.VendorCode;
            v.CountryId = dto.CountryId;
            v.ModifiedOn = DateTime.UtcNow;
            v.ModifiedBy = dto.ModifiedBy;

            await _vendors.UpdateAsync(v, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _vendors.SoftDeleteAsync(id, modifiedBy, ct);
        }

        public async Task<(int added, int updated, int skipped)> ImportAsync(IEnumerable<(string? VendorName, string? VendorCode, string? CountryCode)> rows, Guid userId, CancellationToken ct = default)
        {
            var vendors = await _vendors.GetAllAsync(ct);
            var countries = await _countries.GetAllAsync(ct);
            var vByCode = vendors.Where(v => !string.IsNullOrWhiteSpace(v.VendorCode))
                                 .ToDictionary(v => v.VendorCode!, v => v, StringComparer.OrdinalIgnoreCase);
            var cByCode = countries.Where(c => !string.IsNullOrWhiteSpace(c.CountryCode))
                                   .ToDictionary(c => c.CountryCode!, c => c, StringComparer.OrdinalIgnoreCase);

            int added = 0, updated = 0, skipped = 0;
            foreach (var row in rows)
            {
                var name = (row.VendorName ?? string.Empty).Trim();
                var code = (row.VendorCode ?? string.Empty).Trim();
                var ccode = (row.CountryCode ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(code)) { skipped++; continue; }
                if (!cByCode.TryGetValue(ccode, out var c)) { skipped++; continue; }

                if (vByCode.TryGetValue(code, out var v))
                {
                    v.VendorName = string.IsNullOrWhiteSpace(name) ? v.VendorName : name;
                    v.CountryId = c.Id;
                    v.ModifiedOn = DateTime.UtcNow;
                    v.ModifiedBy = userId;
                    await _vendors.UpdateAsync(v, ct);
                    updated++;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    var nv = new Domain.Vendors.Vendor
                    {
                        Id = Guid.NewGuid(),
                        VendorName = name,
                        VendorCode = code,
                        CountryId = c.Id,
                        IsDeleted = false,
                        CreatedOn = now,
                        ModifiedOn = now,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    };
                    await _vendors.AddAsync(nv, ct);
                    vByCode[code] = nv;
                    added++;
                }
            }
            return (added, updated, skipped);
        }
    }
}
