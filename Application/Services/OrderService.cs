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
        private readonly IRealtimeService _realtimeService;
        private readonly INotificationService _notificationService;

        public OrderService(IUnitOfWork uow, IRealtimeService realtimeService, INotificationService notificationService)
        {
            _uow = uow;
            _realtimeService = realtimeService;
            _notificationService = notificationService;
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

            // Real-time notifications
            await _realtimeService.NotifyCustomerOrderStatusChangedAsync(accountId, order.Id, order.Status);
            await _notificationService.CreateNotificationAsync(accountId, "Đơn hàng mới", $"Đơn hàng #{order.Id} của bạn đã được tạo thành công.", "success");
            await _realtimeService.BroadcastAdminDashboardUpdateAsync("NewOrder", order.Id);
            
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
                .Where(o => o.Status == "Pending" || o.Status == "AwaitingCash");
            return await query
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task UpdateOrderStatusAsync(int orderId, string status, CancellationToken ct = default)
        {
            var order = await _uow.OrderRepository.GetByIdAsync(orderId);
            if (order != null)
            {
                order.Status = status;
                order.ModifiedAt = DateTime.UtcNow;
                order.ModifiedBy = "System_SePay";
                
                await _uow.SaveChangesAsync();
                
                await _realtimeService.NotifyCustomerOrderStatusChangedAsync(order.AccountId, order.Id, order.Status);
                await _notificationService.CreateNotificationAsync(order.AccountId, "Cập nhật đơn hàng", $"Đơn hàng #{order.Id} của bạn đã được cập nhật sang trạng thái: {status}", "info");
                await _realtimeService.BroadcastAdminDashboardUpdateAsync("OrderUpdated", order.Id);
            }
        }
    }
}
