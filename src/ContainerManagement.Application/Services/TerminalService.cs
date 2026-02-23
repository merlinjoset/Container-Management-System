using ContainerManagement.Application.Abstractions;
using ContainerManagement.Application.Dtos.Terminals;
using ContainerManagement.Domain.Terminals;

namespace ContainerManagement.Application.Services
{
    public class TerminalService
    {
        private readonly ITerminalsRepository _repository;
        private readonly IPortsRepository _portsRepository;

        public TerminalService(ITerminalsRepository repository, IPortsRepository portsRepository)
        {
            _repository = repository;
            _portsRepository = portsRepository;
        }

        public async Task<List<TerminalListItemDto>> GetAllAsync(CancellationToken ct = default)
        {
            var terminals = await _repository.GetAllAsync(ct);
            var ports = await _portsRepository.GetAllAsync(ct);
            var portById = ports.ToDictionary(p => p.Id, p => p);

            return terminals.Select(x =>
            {
                portById.TryGetValue(x.PortId, out var port);
                return new TerminalListItemDto
                {
                    Id = x.Id,
                    PortId = x.PortId,
                    TerminalName = x.TerminalName,
                    TerminalCode = x.TerminalCode,
                    PortCode = port?.PortCode,
                    PortName = port?.FullName
                };
            }).ToList();
        }

        public async Task<Guid> CreateAsync(TerminalCreateDto dto, CancellationToken ct = default)
        {
            if (await _repository.ExistsAsync(dto.TerminalCode, null, ct))
                throw new Exception("Terminal code already exists.");

            var now = DateTime.UtcNow;
            var terminal = new Terminal
            {
                Id = Guid.NewGuid(),
                PortId = dto.PortId,
                TerminalName = dto.TerminalName,
                TerminalCode = dto.TerminalCode,
                IsDeleted = false,
                CreatedOn = now,
                ModifiedOn = now,
                CreatedBy = dto.CreatedBy,
                ModifiedBy = dto.CreatedBy
            };

            await _repository.AddAsync(terminal, ct);
            return terminal.Id;
        }

        public async Task UpdateAsync(TerminalUpdateDto dto, CancellationToken ct = default)
        {
            var terminal = await _repository.GetByIdAsync(dto.Id, ct);
            if (terminal == null)
                throw new Exception("Terminal not found.");

            if (await _repository.ExistsAsync(dto.TerminalCode, dto.Id, ct))
                throw new Exception("Terminal code already exists.");

            terminal.PortId = dto.PortId;
            terminal.TerminalName = dto.TerminalName;
            terminal.TerminalCode = dto.TerminalCode;
            terminal.ModifiedOn = DateTime.UtcNow;
            terminal.ModifiedBy = dto.ModifiedBy;

            await _repository.UpdateAsync(terminal, ct);
        }

        public async Task DeleteAsync(Guid id, Guid modifiedBy, CancellationToken ct = default)
        {
            await _repository.SoftDeleteAsync(id, modifiedBy, ct);
        }
    }
}
