using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Vessels;
using ContainerManagement.Domain.Vessels;

namespace ContainerManagement.Application.Services
{
    public class VesselService
    {
        private readonly IVesselsRepository _repository;

        public VesselService(IVesselsRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<VesselListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var vessels = await _repository.GetAllAsync(ct);
            return vessels.Select(x => new VesselListItemDto
            {
                Id = x.Id,
                VesselName = x.VesselName,
                VesselCode = x.VesselCode,
                ImoCode = x.ImoCode,
                Teus = x.Teus,
                NRT = x.NRT,
                GRT = x.GRT,
                Flag = x.Flag,
                Speed = x.Speed,
                BuildYear = x.BuildYear
            }).ToList();
        }

        public async Task<Guid> CreateAsync(VesselCreateDto dto, CancellationToken ct = default)
        {
            if (await _repository.ExistsAsync(dto.VesselCode, null, ct))
                throw new Exception("Vessel code already exists.");

            var now = DateTime.UtcNow;
            var vessel = new Vessel
            {
                Id = Guid.NewGuid(),
                VesselName = dto.VesselName,
                VesselCode = dto.VesselCode,
                ImoCode = dto.ImoCode,
                Teus = dto.Teus,
                NRT = dto.NRT,
                GRT = dto.GRT,
                Flag = dto.Flag,
                Speed = dto.Speed,
                BuildYear = dto.BuildYear,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _repository.AddAsync(vessel, ct);
            return vessel.Id;
        }

        public async Task UpdateAsync(VesselUpdateDto dto, CancellationToken ct = default)
        {
            var vessel = await _repository.GetByIdAsync(dto.Id, ct);
            if (vessel == null)
                throw new Exception("Vessel not found.");

            if (await _repository.ExistsAsync(dto.VesselCode, dto.Id, ct))
                throw new Exception("Vessel code already exists.");

            vessel.VesselName = dto.VesselName;
            vessel.VesselCode = dto.VesselCode;
            vessel.ImoCode = dto.ImoCode;
            vessel.Teus = dto.Teus;
            vessel.NRT = dto.NRT;
            vessel.GRT = dto.GRT;
            vessel.Flag = dto.Flag;
            vessel.Speed = dto.Speed;
            vessel.BuildYear = dto.BuildYear;
            vessel.ModifiedOn = DateTime.UtcNow;
            vessel.ModifiedBy = dto.ModifiedBy;

            await _repository.UpdateAsync(vessel, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _repository.SoftDeleteAsync(id, modifiedBy, ct);
        }

        public async Task<(int added, int updated, int skipped)> ImportAsync(IEnumerable<(string? Name, string? Code, string? Imo, int? Teus, decimal? Nrt, decimal? Grt, string? Flag, decimal? Speed, int? Year)> rows, Guid userId, CancellationToken ct = default)
        {
            var existing = await _repository.GetAllAsync(ct);
            var byCode = existing.Where(v => !string.IsNullOrWhiteSpace(v.VesselCode))
                                 .ToDictionary(v => v.VesselCode!, v => v, StringComparer.OrdinalIgnoreCase);
            int added = 0, updated = 0, skipped = 0;
            foreach (var row in rows)
            {
                var name = (row.Name ?? string.Empty).Trim();
                var code = (row.Code ?? string.Empty).Trim();
                var imo = (row.Imo ?? string.Empty).Trim();
                if (string.IsNullOrWhiteSpace(code)) { skipped++; continue; }

                if (byCode.TryGetValue(code, out var v))
                {
                    v.VesselName = string.IsNullOrWhiteSpace(name) ? v.VesselName : name;
                    v.ImoCode = string.IsNullOrWhiteSpace(imo) ? v.ImoCode : imo;
                    v.Teus = row.Teus ?? v.Teus;
                    v.NRT = row.Nrt ?? v.NRT;
                    v.GRT = row.Grt ?? v.GRT;
                    v.Flag = string.IsNullOrWhiteSpace(row.Flag) ? v.Flag : row.Flag;
                    v.Speed = row.Speed ?? v.Speed;
                    v.BuildYear = row.Year ?? v.BuildYear;
                    v.ModifiedOn = DateTime.UtcNow;
                    v.ModifiedBy = userId;
                    await _repository.UpdateAsync(v, ct);
                    updated++;
                }
                else
                {
                    var now = DateTime.UtcNow;
                    var nv = new Domain.Vessels.Vessel
                    {
                        Id = Guid.NewGuid(),
                        VesselName = name,
                        VesselCode = code,
                        ImoCode = imo,
                        Teus = row.Teus,
                        NRT = row.Nrt,
                        GRT = row.Grt,
                        Flag = row.Flag,
                        Speed = row.Speed,
                        BuildYear = row.Year,
                        IsDeleted = false,
                        CreatedOn = now,
                        ModifiedOn = now,
                        CreatedBy = userId,
                        ModifiedBy = userId
                    };
                    await _repository.AddAsync(nv, ct);
                    byCode[code] = nv;
                    added++;
                }
            }
            return (added, updated, skipped);
        }
    }
}
