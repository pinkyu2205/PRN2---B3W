using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Threading.Tasks;

namespace YukiSoraShop.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                // Manage user-specific group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{userId}");
            }
            
            await base.OnDisconnectedAsync(exception);
        }

        // Clients can call this if they need to join a specific Shop's group (e.g., vendor mode)
        public async Task JoinShopGroup(string shopId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Shop_{shopId}");
        }

        public async Task LeaveShopGroup(string shopId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Shop_{shopId}");
        }
    }
}
