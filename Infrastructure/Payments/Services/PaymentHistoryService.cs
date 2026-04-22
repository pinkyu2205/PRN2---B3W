using Application;
using Application.Payments.DTOs;
using Application.Payments.Interfaces;
using Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Payments.Services
{
    public class PaymentHistoryService : IPaymentHistoryService
    {
        private readonly IUnitOfWork _uow;

        public PaymentHistoryService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(int accountId, CancellationToken ct = default)
        {
            // Use GetAllQueryable and then filter
            var payments = await _uow.PaymentRepository
                .GetAllQueryable("PaymentMethod,Order")
                .Where(p => p.Order.AccountId == accountId && !p.IsDeleted)
                .ToListAsync(ct);

            return payments
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentHistoryDto
                {
                    PaymentId = p.Id,
                    OrderId = p.OrderId,
                    PaymentMethod = p.PaymentMethod?.Name ?? "Unknown",
                    Amount = p.Amount,
                    Currency = p.Currency ?? "VND",
                    Status = p.PaymentStatus,
                    TransactionRef = p.TransactionRef,
                    CreatedAt = p.CreatedAt,
                    OrderTotal = p.Order?.GrandTotal ?? p.Amount,
                    OrderStatus = p.Order?.Status ?? "Unknown"
                })
                .ToList();
        }

        public async Task<List<PaymentHistoryDto>> GetAllPaymentHistoryAsync(
            int pageNumber = 1,
            int pageSize = 50,
            string? search = null,
            string? method = null,
            PaymentStatus? status = null,
            int? orderId = null,
            CancellationToken ct = default)
        {
            var query = _uow.PaymentRepository
                .GetAllQueryable("PaymentMethod,Order,Order.Account")
                .Where(p => !p.IsDeleted);

            if (orderId.HasValue && orderId.Value > 0)
            {
                query = query.Where(p => p.OrderId == orderId.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim();
                query = query.Where(p =>
                    EF.Functions.Like(p.Id.ToString(), $"%{keyword}%")
                    || EF.Functions.Like(p.OrderId.ToString(), $"%{keyword}%")
                    || (!string.IsNullOrEmpty(p.TransactionRef) && EF.Functions.Like(p.TransactionRef, $"%{keyword}%")));
            }

            if (!string.IsNullOrWhiteSpace(method))
            {
                query = query.Where(p => p.PaymentMethod != null && p.PaymentMethod.Name == method);
            }

            if (status.HasValue)
            {
                query = query.Where(p => p.PaymentStatus == status.Value);
            }

            var safePage = pageNumber <= 0 ? 1 : pageNumber;
            var safePageSize = pageSize <= 0 ? 50 : pageSize;

            var payments = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((safePage - 1) * safePageSize)
                .Take(safePageSize)
                .ToListAsync(ct);

            return payments
                .Select(p => new PaymentHistoryDto
                {
                    PaymentId = p.Id,
                    OrderId = p.OrderId,
                    PaymentMethod = p.PaymentMethod?.Name ?? "Unknown",
                    Amount = p.Amount,
                    Currency = p.Currency ?? "VND",
                    Status = p.PaymentStatus,
                    TransactionRef = p.TransactionRef,
                    CreatedAt = p.CreatedAt,
                    OrderTotal = p.Order?.GrandTotal ?? p.Amount,
                    OrderStatus = p.Order?.Status ?? "Unknown"
                })
                .ToList();
        }

        public async Task<PaymentHistoryDto?> GetPaymentByIdAsync(int paymentId, CancellationToken ct = default)
        {
            var payment = await _uow.PaymentRepository
                .FindOneAsync(
                    p => p.Id == paymentId && !p.IsDeleted,
                    includeProperties: "PaymentMethod,Order"
                );

            if (payment == null) return null;

            return new PaymentHistoryDto
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                PaymentMethod = payment.PaymentMethod?.Name ?? "Unknown",
                Amount = payment.Amount,
                Currency = payment.Currency ?? "VND",
                Status = payment.PaymentStatus,
                TransactionRef = payment.TransactionRef,
                CreatedAt = payment.CreatedAt,
                OrderTotal = payment.Order?.GrandTotal ?? payment.Amount,
                OrderStatus = payment.Order?.Status ?? "Unknown"
            };
        }
    }
}
