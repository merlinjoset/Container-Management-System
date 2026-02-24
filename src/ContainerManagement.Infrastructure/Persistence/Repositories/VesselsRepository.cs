using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Vessels;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class VesselsRepository : IVesselsRepository
    {
        private readonly AppDbContext _context;

        public VesselsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Vessel>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<VesselEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.VesselName)
                .Select(x => new Vessel
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
                    BuildYear = x.BuildYear,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<Vessel?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<VesselEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new Vessel
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
                    BuildYear = x.BuildYear,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(string vesselCode, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<VesselEntity>()
                .Where(x => x.VesselCode == vesselCode && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Vessel vessel, CancellationToken ct = default)
        {
            var entity = new VesselEntity
            {
                Id = vessel.Id == Guid.Empty ? Guid.NewGuid() : vessel.Id,
                VesselName = vessel.VesselName,
                VesselCode = vessel.VesselCode,
                ImoCode = vessel.ImoCode,
                Teus = vessel.Teus,
                NRT = vessel.NRT,
                GRT = vessel.GRT,
                Flag = vessel.Flag,
                Speed = vessel.Speed,
                BuildYear = vessel.BuildYear,
                IsDeleted = false,
                CreatedOn = vessel.CreatedOn == default ? DateTime.UtcNow : vessel.CreatedOn,
                ModifiedOn = vessel.ModifiedOn == default ? DateTime.UtcNow : vessel.ModifiedOn,
                CreatedBy = vessel.CreatedBy,
                ModifiedBy = vessel.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            vessel.Id = entity.Id;
        }

        public async Task UpdateAsync(Vessel vessel, CancellationToken ct = default)
        {
            var entity = await _context.Set<VesselEntity>()
                .FirstOrDefaultAsync(x => x.Id == vessel.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Vessel not found.");

            entity.VesselName = vessel.VesselName;
            entity.VesselCode = vessel.VesselCode;
            entity.ImoCode = vessel.ImoCode;
            entity.Teus = vessel.Teus;
            entity.NRT = vessel.NRT;
            entity.GRT = vessel.GRT;
            entity.Flag = vessel.Flag;
            entity.Speed = vessel.Speed;
            entity.BuildYear = vessel.BuildYear;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = vessel.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<VesselEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Vessel not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}

