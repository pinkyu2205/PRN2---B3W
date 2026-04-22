using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace YukiSoraShop.Hubs
{
    // Cấp quyền truy cập công khai (Dành cho trang xem sản phẩm Catalog chung)
    public class CatalogHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            // Join a global product watcher group if needed, or we just broadcast to All
            await Groups.AddToGroupAsync(Context.ConnectionId, "GlobalCatalogWatcher");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(System.Exception? exception)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "GlobalCatalogWatcher");
            await base.OnDisconnectedAsync(exception);
        }
    }
}
