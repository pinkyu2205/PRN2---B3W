namespace Application.Admin.DTOs
{
    public class AdminDashboardSummary
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public int AwaitingCashOrders { get; set; }
    }
}
