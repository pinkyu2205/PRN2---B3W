using Application.Payments.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Payments.Interfaces
{
    public interface IPaymentOrchestrator
    {
        Task<PaymentCheckoutDTO> CreateCheckoutAsync(CreatePaymentCommand command, CancellationToken ct = default);

        Task<PaymentResultDTO> HandleCallbackAsync(IQueryCollection query, CancellationToken ct = default);

        Task<PaymentResultDTO> CreateCashPaymentAsync(int orderId, string createdBy, CancellationToken ct = default);

        Task<PaymentResultDTO> ConfirmCashPaymentAsync(int orderId, string confirmedBy, CancellationToken ct = default);
    }
}
