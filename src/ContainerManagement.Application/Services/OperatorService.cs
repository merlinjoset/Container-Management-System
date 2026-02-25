using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Operators;
using ContainerManagement.Domain.Operators;

namespace ContainerManagement.Application.Services
{
    public class OperatorService
    {
        private readonly IOperatorsRepository _operators;
        private readonly IVendorsRepository _vendors;
        private readonly ICountriesRepository _countries;

        public OperatorService(IOperatorsRepository operators, IVendorsRepository vendors, ICountriesRepository countries)
        {
            _operators = operators;
            _vendors = vendors;
            _countries = countries;
        }

        public async Task<List<OperatorListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var ops = await _operators.GetAllAsync(ct);
            var vendors = await _vendors.GetAllAsync(ct);
            var countries = await _countries.GetAllAsync(ct);
            var vendorById = vendors.ToDictionary(v => v.Id, v => v);
            var countryById = countries.ToDictionary(c => c.Id, c => c);

            return ops.Select(o => new OperatorListItemDto
            {
                Id = o.Id,
                OperatorName = o.OperatorName,
                VendorId = o.VendorId,
                CountryId = o.CountryId,
                VendorName = vendorById.TryGetValue(o.VendorId, out var v) ? v.VendorName : null,
                CountryName = countryById.TryGetValue(o.CountryId, out var c) ? c.Country : null
            }).ToList();
        }

        public async Task<Guid> CreateAsync(OperatorCreateDto dto, CancellationToken ct = default)
        {
            var now = DateTime.UtcNow;
            var op = new Operator
            {
                Id = Guid.NewGuid(),
                OperatorName = dto.OperatorName,
                VendorId = dto.VendorId,
                CountryId = dto.CountryId,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _operators.AddAsync(op, ct);
            return op.Id;
        }

        public async Task UpdateAsync(OperatorUpdateDto dto, CancellationToken ct = default)
        {
            var op = await _operators.GetByIdAsync(dto.Id, ct);
            if (op == null) throw new Exception("Operator not found.");

            op.OperatorName = dto.OperatorName;
            op.VendorId = dto.VendorId;
            op.CountryId = dto.CountryId;
            op.ModifiedOn = DateTime.UtcNow;
            op.ModifiedBy = dto.ModifiedBy;

            await _operators.UpdateAsync(op, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _operators.SoftDeleteAsync(id, modifiedBy, ct);
        }
    }
}
