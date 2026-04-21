namespace Application.Payments.Interfaces
{
    public interface IPaymentQueryService
    {
        Task<decimal> GetTotalRevenueAsync(CancellationToken ct = default);
    }
}

