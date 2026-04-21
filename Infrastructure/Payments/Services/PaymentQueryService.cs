using Application;
using Application.Payments.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Payments.Services
{
    public sealed class PaymentQueryService : IPaymentQueryService
    {
        private readonly IUnitOfWork _uow;

        public PaymentQueryService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default)
        {
            var query = _uow.PaymentRepository.GetAllQueryable()
                .AsNoTracking()
                .Where(p => p.PaymentStatus == Domain.Enums.PaymentStatus.Paid);
            var sum = await query.SumAsync(p => (decimal?)p.Amount, ct) ?? 0m;
            return sum;
        }
    }
}
