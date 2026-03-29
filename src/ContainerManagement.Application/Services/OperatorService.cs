using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Operators;
using ContainerManagement.Domain.Operators;

namespace ContainerManagement.Application.Services
{
    public class OperatorService
    {
        private readonly IOperatorsRepository _operators;
        private readonly IVendorsRepository _vendors;

        public OperatorService(IOperatorsRepository operators, IVendorsRepository vendors)
        {
            _operators = operators;
            _vendors = vendors;
        }

        public async Task<List<OperatorListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var ops = await _operators.GetAllAsync(ct);
            var vendors = await _vendors.GetAllAsync(ct);
            var vendorById = vendors.ToDictionary(v => v.Id, v => v);

            return ops.Select(o => new OperatorListItemDto
            {
                Id = o.Id,
                OperatorName = o.OperatorName,
                VendorId = o.VendorId,
                VendorName = vendorById.TryGetValue(o.VendorId, out var v) ? v.VendorName : null,
                IsCompetitor = o.IsCompetitor
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
                IsCompetitor = dto.IsCompetitor,
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
            op.IsCompetitor = dto.IsCompetitor;
            op.ModifiedOn = DateTime.UtcNow;
            op.ModifiedBy = dto.ModifiedBy;

            await _operators.UpdateAsync(op, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _operators.SoftDeleteAsync(id, modifiedBy, ct);
        }

        public async Task<(int added, int updated, int skipped)> ImportAsync(IEnumerable<(string? OperatorName, string? VendorCode, bool IsCompetitor)> rows, Guid userId, CancellationToken ct = default)
        {
            var ops = await _operators.GetAllAsync(ct);
            var vendors = await _vendors.GetAllAsync(ct);
            var vByCode = vendors.Where(v => !string.IsNullOrWhiteSpace(v.VendorCode))
                                 .ToDictionary(v => v.VendorCode!, v => v, StringComparer.OrdinalIgnoreCase);

            // Use operator name + vendor as identity
            var key = static (string name, Guid vendorId) => $"{name.ToLowerInvariant()}|{vendorId}";
            var opByKey = ops.Where(o => !string.IsNullOrWhiteSpace(o.OperatorName))
                             .ToDictionary(o => key(o.OperatorName!, o.VendorId), o => o);

            int added = 0, updated = 0, skipped = 0;
            foreach (var row in rows)
            {
                var name = (row.OperatorName ?? string.Empty).Trim();
                var vcode = (row.VendorCode ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(name)) { skipped++; continue; }
                if (!vByCode.TryGetValue(vcode, out var vendor)) { skipped++; continue; }

                var k = key(name, vendor.Id);
                if (opByKey.TryGetValue(k, out var op))
                {
                    op.IsCompetitor = row.IsCompetitor;
                    op.ModifiedOn = DateTime.UtcNow;
                    op.ModifiedBy = userId;
                    await _operators.UpdateAsync(op, ct);
                    updated++;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    var no = new Domain.Operators.Operator
                    {
                        Id = Guid.NewGuid(),
                        OperatorName = name,
                        VendorId = vendor.Id,
                        IsCompetitor = row.IsCompetitor,
                        IsDeleted = false,
                        CreatedOn = now,
                        ModifiedOn = now,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    };
                    await _operators.AddAsync(no, ct);
                    opByKey[k] = no;
                    added++;
                }
            }
            return (added, updated, skipped);
        }
    }
}
