using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderFromCartAsync(int accountId, IEnumerable<OrderItemInput> items, string createdBy, CancellationToken ct = default);
        Task<int> GetTotalOrdersAsync();
        Task<Order?> GetOrderByIdAsync(int id);
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<List<Order>> GetOrdersAwaitingCashAsync(CancellationToken ct = default);
    }
}
