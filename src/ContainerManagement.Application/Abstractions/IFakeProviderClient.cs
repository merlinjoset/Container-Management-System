namespace ContainerManagement.Application.Abstractions;

public interface IFakeProviderClient
{
    Task<FakeProviderResult> PayAsync(Guid paymentId, decimal amount, string currency, string reference, CancellationToken ct);
}

public record FakeProviderResult(bool Success, string? ProviderRef, string? Error);
