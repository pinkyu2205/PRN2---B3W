using System.Threading.Tasks;

namespace Application.Services.Interfaces
{
    public interface IRealtimeService
    {
        /// <summary>
        /// Sends a notification to a specific user.
        /// </summary>
        Task SendNotificationToUserAsync(int userId, string title, string message, string type = "info");

        /// <summary>
        /// Sends a notification to all users in a specific role.
        /// </summary>
        Task SendNotificationToRoleAsync(string role, string title, string message, string type = "info");

        /// <summary>
        /// Sends a notification to a specific shop owner.
        /// </summary>
        Task NotifyShopNewOrderAsync(int shopId, int orderId, string message);

        /// <summary>
        /// Notifies a customer that their order status has changed.
        /// </summary>
        Task NotifyCustomerOrderStatusChangedAsync(int customerId, int orderId, string newStatus);

        /// <summary>
        /// Global broadcast about product stock change (to update catalog dynamically).
        /// </summary>
        Task BroadcastProductStockChangedAsync(int productId, int currentStock);
        
        /// <summary>
        /// Pushes a global event to the admin dashboard.
        /// </summary>
        Task BroadcastAdminDashboardUpdateAsync(string statKey, object statValue);
    }
}
