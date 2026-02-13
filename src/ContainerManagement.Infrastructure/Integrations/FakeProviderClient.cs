using System.Net.Http.Json;
using ContainerManagement.Application.Abstractions;

namespace ContainerManagement.Infrastructure.Integrations;

public class FakeProviderClient : IFakeProviderClient
{
    private readonly HttpClient _http;
    public FakeProviderClient(HttpClient http) => _http = http;

    public async Task<FakeProviderResult> PayAsync(Guid paymentId, decimal amount, string currency, string reference, CancellationToken ct)
    {
        var resp = await _http.PostAsJsonAsync("/fake-provider/pay", new { paymentId, amount, currency, reference }, ct);
        var result = await resp.Content.ReadFromJsonAsync<FakeProviderResult>(cancellationToken: ct);
        return result ?? new FakeProviderResult(false, null, "Invalid provider response.");
    }
}
