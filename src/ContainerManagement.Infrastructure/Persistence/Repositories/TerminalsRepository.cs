using ContainerManagement.Application.Abstractions;
using ContainerManagement.Domain.Terminals;
using ContainerManagement.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ContainerManagement.Infrastructure.Persistence.Repositories
{
    public class TerminalsRepository : ITerminalsRepository
    {
        private readonly AppDbContext _context;

        public TerminalsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Terminal>> GetAllAsync(CancellationToken ct = default)
        {
            return await _context.Set<TerminalEntity>()
                .AsNoTracking()
                .Where(x => !x.IsDeleted)
                .OrderBy(x => x.TerminalName)
                .Select(x => new Terminal
                {
                    Id = x.Id,
                    PortId = x.PortId,
                    TerminalName = x.TerminalName,
                    TerminalCode = x.TerminalCode,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .ToListAsync(ct);
        }

        public async Task<Terminal?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return await _context.Set<TerminalEntity>()
                .AsNoTracking()
                .Where(x => x.Id == id && !x.IsDeleted)
                .Select(x => new Terminal
                {
                    Id = x.Id,
                    PortId = x.PortId,
                    TerminalName = x.TerminalName,
                    TerminalCode = x.TerminalCode,
                    IsDeleted = x.IsDeleted,
                    CreatedOn = x.CreatedOn,
                    ModifiedOn = x.ModifiedOn,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy
                })
                .FirstOrDefaultAsync(ct);
        }

        public async Task<bool> ExistsAsync(string terminalCode, Guid? excludeId = null, CancellationToken ct = default)
        {
            var query = _context.Set<TerminalEntity>()
                .Where(x => x.TerminalCode == terminalCode && !x.IsDeleted);

            if (excludeId.HasValue)
                query = query.Where(x => x.Id != excludeId.Value);

            return await query.AnyAsync(ct);
        }

        public async Task AddAsync(Terminal terminal, CancellationToken ct = default)
        {
            var entity = new TerminalEntity
            {
                Id = terminal.Id == Guid.Empty ? Guid.NewGuid() : terminal.Id,
                PortId = terminal.PortId,
                TerminalName = terminal.TerminalName,
                TerminalCode = terminal.TerminalCode,
                IsDeleted = false,
                CreatedOn = terminal.CreatedOn == default ? DateTime.UtcNow : terminal.CreatedOn,
                ModifiedOn = terminal.ModifiedOn == default ? DateTime.UtcNow : terminal.ModifiedOn,
                CreatedBy = terminal.CreatedBy,
                ModifiedBy = terminal.ModifiedBy
            };

            await _context.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);

            terminal.Id = entity.Id;
        }

        public async Task UpdateAsync(Terminal terminal, CancellationToken ct = default)
        {
            var entity = await _context.Set<TerminalEntity>()
                .FirstOrDefaultAsync(x => x.Id == terminal.Id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Terminal not found.");

            entity.PortId = terminal.PortId;
            entity.TerminalName = terminal.TerminalName;
            entity.TerminalCode = terminal.TerminalCode;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = terminal.ModifiedBy;

            await _context.SaveChangesAsync(ct);
        }

        public async Task SoftDeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            var entity = await _context.Set<TerminalEntity>()
                .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);

            if (entity == null)
                throw new KeyNotFoundException("Terminal not found.");

            entity.IsDeleted = true;
            entity.ModifiedOn = DateTime.UtcNow;
            entity.ModifiedBy = modifiedBy;

            await _context.SaveChangesAsync(ct);
        }
    }
}

