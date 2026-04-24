using Application.DTOs;
using Application.Services.Interfaces;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _uow;
        private readonly IRealtimeService _realtimeService;

        public NotificationService(IUnitOfWork uow, IRealtimeService realtimeService)
        {
            _uow = uow;
            _realtimeService = realtimeService;
        }

        public async Task<List<NotificationDTO>> GetUserNotificationsAsync(int accountId, int count = 20)
        {
            var query = _uow.NotificationRepository.GetAllQueryable()
                .Where(n => n.AccountId == accountId || n.AccountId == null)
                .OrderByDescending(n => n.CreatedAt)
                .Take(count);

            var entities = await Task.FromResult(query.ToList()); // Evaluate synchronously since GetAllQueryable isn't async
            return entities.Select(n => new NotificationDTO
            {
                Id = n.Id,
                AccountId = n.AccountId,
                Title = n.Title,
                Message = n.Message,
                Type = n.Type,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                TimeAgo = GetTimeAgo(n.CreatedAt)
            }).ToList();
        }

        public async Task<int> GetUnreadCountAsync(int accountId)
        {
            var count = _uow.NotificationRepository.GetAllQueryable()
                .Count(n => (n.AccountId == accountId || n.AccountId == null) && !n.IsRead);
            return await Task.FromResult(count);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int accountId)
        {
            var noti = await _uow.NotificationRepository.GetByIdAsync(notificationId);
            if (noti != null && (noti.AccountId == accountId || noti.AccountId == null))
            {
                noti.IsRead = true;
                _uow.NotificationRepository.Update(noti);
                await _uow.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> MarkAllAsReadAsync(int accountId)
        {
            var query = _uow.NotificationRepository.GetAllQueryable()
                .Where(n => (n.AccountId == accountId || n.AccountId == null) && !n.IsRead);
                
            var unread = query.ToList();
            if (unread.Any())
            {
                foreach (var noti in unread)
                {
                    noti.IsRead = true;
                    _uow.NotificationRepository.Update(noti);
                }
                await _uow.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> CreateNotificationAsync(int? accountId, string title, string message, string type = "info")
        {
            var noti = new Notification
            {
                AccountId = accountId,
                Title = title,
                Message = message,
                Type = type,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            await _uow.NotificationRepository.AddAsync(noti);
            var result = await _uow.SaveChangesAsync();

            if (result > 0)
            {
                // Push real-time
                if (accountId.HasValue)
                {
                    await _realtimeService.SendNotificationToUserAsync(accountId.Value, title, message, type);
                }
                else
                {
                    // If accountId is null, broadcast to all.
                    await _realtimeService.BroadcastNotificationToAllAsync(title, message, type);
                }
                return true;
            }
            return false;
        }

        private string GetTimeAgo(DateTime dt)
        {
            var ts = new TimeSpan(DateTime.UtcNow.Ticks - dt.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 60) return ts.Seconds == 1 ? "1 giây trước" : ts.Seconds + " giây trước";
            if (delta < 120) return "1 phút trước";
            if (delta < 2700) return ts.Minutes + " phút trước";
            if (delta < 5400) return "1 giờ trước";
            if (delta < 86400) return ts.Hours + " giờ trước";
            if (delta < 172800) return "Hôm qua";
            if (delta < 2592000) return ts.Days + " ngày trước";
            if (delta < 31104000)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                return months <= 1 ? "1 tháng trước" : months + " tháng trước";
            }
            int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
            return years <= 1 ? "1 năm trước" : years + " năm trước";
        }
    }
}
