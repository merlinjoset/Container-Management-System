namespace ContainerManagement.Application.Abstractions;

public interface IUnitOfWork
{
    Task ExecuteAsync(Func<Task> action, CancellationToken ct);
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken ct);
}
