namespace GMS.Application.Interfaces;

public interface IRealTimeNotifier
{
    Task NotifyUserAsync(int userId, string title, string message);
    Task NotifyRoleAsync(string role, string title, string message);
}
