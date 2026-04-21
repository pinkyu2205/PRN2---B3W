using Domain.Entities;

namespace Application.Services.Interfaces
{
    public interface IPaymentMethodService
    {
        Task<List<PaymentMethod>> GetAllAsync(CancellationToken ct = default);
        Task<List<PaymentMethod>> GetActiveAsync(CancellationToken ct = default);
        Task<bool> SetStatusAsync(int id, bool isActive, string modifiedBy, CancellationToken ct = default);
    }
}
