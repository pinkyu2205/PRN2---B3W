using Application.Admin.DTOs;

namespace Application.Admin.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardSummary> GetSummaryAsync(CancellationToken ct = default);
        Task<AdminDashboardSummary> RefreshSummaryAsync(CancellationToken ct = default);
    }
}
