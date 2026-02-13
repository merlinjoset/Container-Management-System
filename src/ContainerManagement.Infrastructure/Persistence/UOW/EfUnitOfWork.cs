using ContainerManagement.Application.Abstractions;

namespace ContainerManagement.Infrastructure.Persistence.Uow;

public class EfUnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;
    public EfUnitOfWork(AppDbContext db) => _db = db;

    public async Task ExecuteAsync(Func<Task> action, CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try { await action(); await tx.CommitAsync(ct); }
        catch { await tx.RollbackAsync(ct); throw; }
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken ct)
    {
        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try { var r = await action(); await tx.CommitAsync(ct); return r; }
        catch { await tx.RollbackAsync(ct); throw; }
    }
}
