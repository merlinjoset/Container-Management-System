using System.Security.Claims;
using ContainerManagement.Application.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ContainerManagement.Web.Hubs;

[Authorize]
public class PaymentsHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userId, out var uid))
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user:{uid}");

        if (Context.User?.IsInRole(AppRoles.Admin) == true)
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");

        await base.OnConnectedAsync();
    }
}
