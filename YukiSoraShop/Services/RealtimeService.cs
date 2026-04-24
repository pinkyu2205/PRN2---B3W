using Application.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using YukiSoraShop.Hubs;

namespace YukiSoraShop.Services
{
    public class RealtimeService : IRealtimeService
    {
        private readonly IHubContext<NotificationHub> _notificationHub;
        private readonly IHubContext<CatalogHub> _catalogHub;
        private readonly IHubContext<AdminDashboardHub> _adminDashboardHub;

        public RealtimeService(
            IHubContext<NotificationHub> notificationHub,
            IHubContext<CatalogHub> catalogHub,
            IHubContext<AdminDashboardHub> adminDashboardHub)
        {
            _notificationHub = notificationHub;
            _catalogHub = catalogHub;
            _adminDashboardHub = adminDashboardHub;
        }

        public async Task SendNotificationToUserAsync(int userId, string title, string message, string type = "info")
        {
            await _notificationHub.Clients.Group($"User_{userId}").SendAsync("ReceiveNotification", title, message, type);
        }

        public async Task BroadcastNotificationToAllAsync(string title, string message, string type = "info")
        {
            await _notificationHub.Clients.All.SendAsync("ReceiveNotification", title, message, type);
        }

        public async Task SendNotificationToRoleAsync(string role, string title, string message, string type = "info")
        {
            // Simplified: we could manage role groups or broadcast differently. 
            // Currently our system has AdminDashboardHub handling Administrators group.
            if (role == "Administrator")
            {
                await _adminDashboardHub.Clients.All.SendAsync("ReceiveNotification", title, message, type);
            }
        }

        public async Task NotifyShopNewOrderAsync(int shopId, int orderId, string message)
        {
            // Can be picked up by the vendor listening to Shop_shopId
            await _notificationHub.Clients.Group($"Shop_{shopId}").SendAsync("ReceiveOrderNotification", orderId, message);
        }

        public async Task NotifyCustomerOrderStatusChangedAsync(int customerId, int orderId, string newStatus)
        {
            await _notificationHub.Clients.Group($"User_{customerId}").SendAsync("ReceiveOverviewUpdate", orderId, newStatus);
        }

        public async Task BroadcastProductStockChangedAsync(int productId, int currentStock)
        {
            await _catalogHub.Clients.Group("GlobalCatalogWatcher").SendAsync("StockUpdated", productId, currentStock);
        }

        public async Task BroadcastAdminDashboardUpdateAsync(string statKey, object statValue)
        {
            await _adminDashboardHub.Clients.All.SendAsync("AdminDashboardStatsUpdate", statKey, statValue);
        }
    }
}
