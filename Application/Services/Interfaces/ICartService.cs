using Application.DTOs;
using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface ICartService
    {
        // Session/in-memory helpers (for compatibility with existing callers)
        List<CartItemDTO> IncreaseQuantity(List<CartItemDTO> cart, int productId);
        List<CartItemDTO> DecreaseQuantity(List<CartItemDTO> cart, int productId);
        List<CartItemDTO> RemoveItem(List<CartItemDTO> cart, int productId);
        decimal GetTotal(List<CartItemDTO> cart);
        int GetCount(List<CartItemDTO> cart);
        List<OrderItemInput> ToOrderItems(List<CartItemDTO> cart);

        // Persistent cart (Unit of Work + Entities)
        Task<Cart> GetOrCreateCartAsync(int accountId, CancellationToken ct = default);
        Task<IReadOnlyList<CartItem>> GetItemsAsync(int accountId, CancellationToken ct = default);
        Task AddItemAsync(int accountId, int productId, int quantity, CancellationToken ct = default);
        Task UpdateQuantityAsync(int accountId, int productId, int quantity, CancellationToken ct = default);
        Task RemoveItemAsync(int accountId, int productId, CancellationToken ct = default);
        Task ClearAsync(int accountId, CancellationToken ct = default);
        Task<decimal> GetTotalAsync(int accountId, CancellationToken ct = default);
        Task<List<OrderItemInput>> ToOrderItemsAsync(int accountId, CancellationToken ct = default);
    }
}
