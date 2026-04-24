using Application.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface INotificationService
    {
        Task<List<NotificationDTO>> GetUserNotificationsAsync(int accountId, int count = 20);
        Task<int> GetUnreadCountAsync(int accountId);
        Task<bool> MarkAsReadAsync(int notificationId, int accountId);
        Task<bool> MarkAllAsReadAsync(int accountId);
        Task<bool> CreateNotificationAsync(int? accountId, string title, string message, string type = "info");
    }
}
