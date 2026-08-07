using GMS.API.Hubs;
using GMS.Application.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace GMS.API.Services;

public class SignalRNotifier : IRealTimeNotifier
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotifier(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task NotifyUserAsync(int userId, string title, string message)
    {
        await _hubContext.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", new { title = title, message = message });
    }

    public async Task NotifyRoleAsync(string role, string title, string message)
    {
        await _hubContext.Clients.Group(role).SendAsync("ReceiveNotification", new { title = title, message = message });
    }
}
