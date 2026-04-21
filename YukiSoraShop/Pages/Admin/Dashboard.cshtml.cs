using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Application.Admin.Interfaces;

namespace YukiSoraShop.Pages.Admin
{
    [Authorize(Roles = "Administrator")]
    public class AdminDashboardModel : PageModel
    {
        private readonly IAdminDashboardService _dashboard;

        public AdminDashboardModel(IAdminDashboardService dashboard)
        {
            _dashboard = dashboard;
        }

        public int TotalUsers { get; set; } = 0;
        public int TotalProducts { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;
        public decimal TotalRevenue { get; set; } = 0;
        public int AwaitingCashOrders { get; set; } = 0;

        public async Task OnGetAsync()
        {
            var summary = await _dashboard.GetSummaryAsync();
            TotalUsers = summary.TotalUsers;
            TotalProducts = summary.TotalProducts;
            TotalOrders = summary.TotalOrders;
            TotalRevenue = summary.TotalRevenue;
            AwaitingCashOrders = summary.AwaitingCashOrders;
        }
    }
}
