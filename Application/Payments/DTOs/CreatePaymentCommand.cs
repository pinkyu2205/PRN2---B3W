
namespace Application.Payments.DTOs
{
    public sealed class CreatePaymentCommand
    {
        public int OrderId { get; set; }
        public string? BankCode { get; set; } // optional
        public string ClientIp { get; set; } = default!;
        public string? OrderDescription { get; set; }
        public string OrderTypeCode { get; set; } = "other";
    }
}
