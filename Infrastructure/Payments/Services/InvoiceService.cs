using Application;
using Application.Payments.Interfaces;
using Microsoft.EntityFrameworkCore;
using Domain.Entities;


namespace Infrastructure.Payments.Services
{
    public sealed class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _uow;

        public InvoiceService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<Invoice> CreateInvoiceFromOrder(int orderId, CancellationToken ct = default)
        {
            var order = await _uow.OrderRepository
                .FindOneAsync(o => o.Id == orderId, includeProperties: "OrderDetails,OrderDetails.Product");

            if (order == null)
                throw new InvalidOperationException($"Order {orderId} not found.");

            var existed = await _uow.InvoiceRepository
                .GetAllQueryable("InvoiceDetails")
                .AsNoTracking()
                .Where(i => i.OrderId == orderId && i.Status == "Issued")
                .FirstOrDefaultAsync(ct);

            if (existed != null) return existed;

            decimal subtotal = 0m;

            var invoice = new Invoice
            {
                OrderId = order.Id,
                InvoiceNumber = GenerateInvoiceNumber(order.Id),
                Status = "Issued",
                Subtotal = 0m, // set sau
            };

            foreach (var od in order.OrderDetails)
            {
                // Lấy giá và tên tại thời điểm lập hoá đơn (snapshot)
                var unitPrice = od.UnitPrice;
                var qty = od.Quantity;
                var lineTotal = unitPrice * qty;

                invoice.InvoiceDetails.Add(new InvoiceDetail
                {
                    ProductId = od.ProductId, // có thể để null nếu muốn “độc lập” hoàn toàn
                    ProductName = od.Product?.ProductName ?? $"Product#{od.ProductId}",
                    Quantity = qty,
                    UnitPrice = unitPrice,
                    LineTotal = lineTotal
                });

                subtotal += lineTotal;
            }

            invoice.Subtotal = subtotal;

            await _uow.InvoiceRepository.AddAsync(invoice);
            await _uow.SaveChangesAsync();

            return invoice;
        }

        private static string GenerateInvoiceNumber(int orderId)
        {
            // Đảm bảo unique; bạn có thể thay bằng sequence trong DB nếu muốn.
            // Dạng: INV-YYYYMMDD-OrderId-xxxx
            return $"INV-{DateTime.UtcNow:yyyyMMdd}-{orderId}-{Random.Shared.Next(1000, 9999)}";
        }
    }
}

