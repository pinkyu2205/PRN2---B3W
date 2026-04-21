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

        public async Task<List<PaymentHistoryDto>> GetAllPaymentHistoryAsync(int pageNumber = 1, int pageSize = 50, CancellationToken ct = default)
        {
            var payments = await _uow.PaymentRepository
                .GetAllQueryable("PaymentMethod,Order,Order.Account")
                .Where(p => !p.IsDeleted)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
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
