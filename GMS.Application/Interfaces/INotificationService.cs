using GMS.Application.DTOs.Notifications;

namespace GMS.Application.Interfaces;

public interface INotificationService
{
    Task CreateNotificationAsync(int userId, string title, string message);
    Task<IEnumerable<NotificationResponse>> GetNotificationsAsync(int userId);
    Task MarkAsReadAsync(int id, int userId);
    Task MarkAllAsReadAsync(int userId);
    Task DeleteNotificationAsync(int id, int userId);
    Task<int> GetUnreadCountAsync(int userId);
}
