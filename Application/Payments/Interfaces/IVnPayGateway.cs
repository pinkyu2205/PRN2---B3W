using Application.Payments.DTOs;
using Microsoft.AspNetCore.Http;

namespace Application.Payments.Interfaces
{
    public interface IVnPayGateway
    {
        Task<PaymentCheckoutDTO> GenerateCheckoutUrlAsync(
            int orderId,
            decimal amountVnd,
            string clientIp,
            string? bankCode,
            string? orderDesc,
            string orderTypeCode,
            CancellationToken ct = default);

        Task<PaymentResultDTO> ParseAndValidateCallbackAsync(IQueryCollection query, CancellationToken ct = default);
    }
    
    public interface IPaymentHistoryService
    {
        Task<List<PaymentHistoryDto>> GetUserPaymentHistoryAsync(int accountId, CancellationToken ct = default);
        Task<List<PaymentHistoryDto>> GetAllPaymentHistoryAsync(int pageNumber = 1, int pageSize = 50, CancellationToken ct = default);
        Task<PaymentHistoryDto?> GetPaymentByIdAsync(int paymentId, CancellationToken ct = default);
    }
}

