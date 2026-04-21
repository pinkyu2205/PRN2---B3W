using Application.DTOs;
using Application.Services.Interfaces;
using Application;
using Domain.Entities;

namespace Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _uow;

        public CartService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        // In-memory helpers (backward compatibility)
        public List<CartItemDTO> IncreaseQuantity(List<CartItemDTO> cart, int productId)
        {
            cart = cart ?? new List<CartItemDTO>();
            var item = cart.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null) item.Quantity++;
            return cart;
        }

        public List<CartItemDTO> DecreaseQuantity(List<CartItemDTO> cart, int productId)
        {
            cart = cart ?? new List<CartItemDTO>();
            var item = cart.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null && item.Quantity > 1) item.Quantity--;
            return cart;
        }

        public List<CartItemDTO> RemoveItem(List<CartItemDTO> cart, int productId)
        {
            cart = cart ?? new List<CartItemDTO>();
            var item = cart.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null) cart.Remove(item);
            return cart;
        }

        public decimal GetTotal(List<CartItemDTO> cart) => (cart ?? new List<CartItemDTO>()).Sum(i => i.TotalPrice);
        public int GetCount(List<CartItemDTO> cart) => (cart ?? new List<CartItemDTO>()).Sum(i => i.Quantity);
        public List<OrderItemInput> ToOrderItems(List<CartItemDTO> cart) => (cart ?? new List<CartItemDTO>()).Select(ci => new OrderItemInput { ProductId = ci.Product.Id, Quantity = ci.Quantity }).ToList();

        // Persistent cart methods
        public async Task<Cart> GetOrCreateCartAsync(int accountId, CancellationToken ct = default)
        {
            var cart = await _uow.CartRepository.FindOneAsync(c => c.AccountId == accountId, includeProperties: "CartItems,CartItems.Product");
            if (cart == null)
            {
                cart = new Cart
                {
                    AccountId = accountId,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    ModifiedBy = "system",
                    IsDeleted = false
                };
                await _uow.CartRepository.AddAsync(cart);
                await _uow.SaveChangesAsync();
            }
            return cart;
        }

        public async Task<IReadOnlyList<CartItem>> GetItemsAsync(int accountId, CancellationToken ct = default)
        {
            var cart = await _uow.CartRepository.FindOneAsync(c => c.AccountId == accountId, includeProperties: "CartItems,CartItems.Product");
            return cart?.CartItems.Where(ci => !ci.IsDeleted).ToList() ?? new List<CartItem>();
        }

        public async Task AddItemAsync(int accountId, int productId, int quantity, CancellationToken ct = default)
        {
            if (productId <= 0) throw new ArgumentException("ProductId must be greater than 0", nameof(productId));
            if (quantity <= 0) throw new ArgumentException("Quantity must be greater than 0", nameof(quantity));

            var cart = await GetOrCreateCartAsync(accountId, ct);
            
            // Validate product exists
            var product = await _uow.ProductRepository.GetByIdAsync(productId);
            if (product == null)
                throw new InvalidOperationException($"Product with ID {productId} does not exist.");

            // Check if item already exists in cart
            var existing = cart.CartItems.FirstOrDefault(i => i.ProductId == productId && !i.IsDeleted);
            if (existing != null)
            {
                existing.Quantity += quantity;
                existing.ModifiedAt = DateTime.UtcNow;
                existing.ModifiedBy = "system";
            }
            else
            {
                var item = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    CreatedBy = "system",
                    ModifiedBy = "system",
                    IsDeleted = false
                };
                await _uow.CartItemRepository.AddAsync(item);
                cart.CartItems.Add(item);
            }

            cart.ModifiedAt = DateTime.UtcNow;
            await _uow.SaveChangesAsync();
        }

        public async Task UpdateQuantityAsync(int accountId, int productId, int quantity, CancellationToken ct = default)
        {
            var cart = await GetOrCreateCartAsync(accountId, ct);
            var existing = cart.CartItems.FirstOrDefault(i => i.ProductId == productId && !i.IsDeleted);
            if (existing == null) return;
            
            if (quantity <= 0)
            {
                existing.IsDeleted = true;
                existing.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                existing.Quantity = quantity;
                existing.ModifiedAt = DateTime.UtcNow;
            }
            await _uow.SaveChangesAsync();
        }

        public async Task RemoveItemAsync(int accountId, int productId, CancellationToken ct = default)
        {
            await UpdateQuantityAsync(accountId, productId, 0, ct);
        }

        public async Task ClearAsync(int accountId, CancellationToken ct = default)
        {
            var cart = await GetOrCreateCartAsync(accountId, ct);
            foreach (var item in cart.CartItems.Where(i => !i.IsDeleted))
            {
                item.IsDeleted = true;
                item.ModifiedAt = DateTime.UtcNow;
            }
            await _uow.SaveChangesAsync();
        }

        public async Task<decimal> GetTotalAsync(int accountId, CancellationToken ct = default)
        {
            var items = await GetItemsAsync(accountId, ct);
            return items.Sum(i => (i.Product?.Price ?? 0m) * i.Quantity);
        }

        public async Task<List<OrderItemInput>> ToOrderItemsAsync(int accountId, CancellationToken ct = default)
        {
            var items = await GetItemsAsync(accountId, ct);
            return items.Select(i => new OrderItemInput
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList();
        }
    }
}

