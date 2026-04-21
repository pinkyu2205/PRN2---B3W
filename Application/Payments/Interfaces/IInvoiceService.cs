using Domain.Entities;
namespace Application.Payments.Interfaces
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateInvoiceFromOrder(int orderId, CancellationToken ct = default);
    }
}

