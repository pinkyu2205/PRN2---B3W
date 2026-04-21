using Application.DTOs;
using Application.IRepository;
using Application.Services.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _uow;

        public OrderService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Order> CreateOrderFromCartAsync(int accountId, IEnumerable<OrderItemInput> items, string createdBy, CancellationToken ct = default)
        {
            var list = items?.Where(i => i.Quantity > 0).ToList() ?? new List<OrderItemInput>();
            if (list.Count == 0) throw new InvalidOperationException("Cart is empty");

            decimal subtotal = 0m;

            var order = new Order
            {
                AccountId = accountId,
                Status = "Pending",
                Subtotal = 0m,
                ShippingFee = 0m,
                GrandTotal = 0m,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = createdBy,
                ModifiedAt = DateTime.UtcNow,
                ModifiedBy = createdBy,
                IsDeleted = false
            };

            using (var tx = await _uow.BeginTransactionAsync())
            {
                foreach (var item in list)
                {
                    var product = await _uow.ProductRepository.GetByIdAsync(item.ProductId);
                    if (product == null) continue;

                    var unitPrice = product.Price;
                    var lineTotal = unitPrice * item.Quantity;
                    subtotal += lineTotal;

                    var od = new OrderDetail
                    {
                        ProductId = product.Id,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice,
                        LineTotal = lineTotal,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = createdBy,
                        ModifiedAt = DateTime.UtcNow,
                        ModifiedBy = createdBy,
                        IsDeleted = false
                    };

                    order.OrderDetails.Add(od);
                }

            if (order.OrderDetails.Count == 0)
                throw new InvalidOperationException("No valid items to create order.");

            order.Subtotal = subtotal;
            var tax = subtotal * 0.10m;
            order.GrandTotal = subtotal + tax + order.ShippingFee;

                await _uow.OrderRepository.AddAsync(order);
                await _uow.SaveChangesAsync();
                await tx.CommitAsync(ct);
            }
            return order;
        }

        public async Task<int> GetTotalOrdersAsync()
        {
            return await _uow.OrderRepository.GetCountAsync();
        }

        public Task<Order?> GetOrderByIdAsync(int id)
        {
            return _uow.OrderRepository.GetByIdAsync(id);
        }

        public Task<Order?> GetOrderWithDetailsAsync(int id)
        {
            return _uow.OrderRepository.FindOneAsync(o => o.Id == id, "Account,OrderDetails.Product,Payments.PaymentMethod");
        }

        public async Task<List<Order>> GetOrdersAwaitingCashAsync(CancellationToken ct = default)
        {
            var query = _uow.OrderRepository.GetAllQueryable("Account,Payments.PaymentMethod,OrderDetails.Product")
                .AsNoTracking()
                .Where(o => o.Status == "AwaitingCash");
            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);
        }
    }
}
