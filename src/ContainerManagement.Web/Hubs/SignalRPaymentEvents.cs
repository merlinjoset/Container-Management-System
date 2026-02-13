using ContainerManagement.Application.Abstractions;
using Microsoft.AspNetCore.SignalR;

namespace ContainerManagement.Web.Hubs;

public class SignalRPaymentEvents : IPaymentEvents
{
    private readonly IHubContext<PaymentsHub> _hub;

    public SignalRPaymentEvents(IHubContext<PaymentsHub> hub)
    {
        _hub = hub;
    }

    public async Task NotifyPaymentUpdatedAsync(Guid paymentId, Guid userId, CancellationToken ct)
    {
        // notify the owner
        await _hub.Clients.Group($"user:{userId}")
            .SendAsync("paymentUpdated", paymentId, ct);

        // notify admin
        await _hub.Clients.Group("admins")
            .SendAsync("paymentUpdated", paymentId, ct);
    }
}
