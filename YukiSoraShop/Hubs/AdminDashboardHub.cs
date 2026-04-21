using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace YukiSoraShop.Hubs
{
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardHub : Hub
    {
        public const string AdminGroupName = "administrators";

        public override async Task OnConnectedAsync()
        {
            if (Context.User?.IsInRole("Administrator") ?? false)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroupName);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (Context.User?.IsInRole("Administrator") ?? false)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroupName);
            }

            await base.OnDisconnectedAsync(exception);
        }
    }
}
