using Domain.Entities;
using Domain.Enums;

namespace Application.IRepository
{
    public interface IPaymentRepository : IGenericRepository<Payment>
    {
        Task<decimal> GetTotalRevenueAsync(PaymentStatus successStatus);
    }
}
