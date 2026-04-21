
namespace Application.Payments.DTOs
{
    public sealed class PaymentResultDTO
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? TransactionRef { get; set; }
        public string? BankCode { get; set; }
        public string? PayDate { get; set; }
        public string RawQuery { get; set; } = string.Empty;

        // mapping sang domain:
        public decimal Amount { get; set; } // VND
        public string Currency { get; set; } = "VND";
        public Domain.Enums.PaymentStatus Status { get; set; }
        public int OrderId { get; set; } // VNPay không gắn OrderId, ta tự encode vào vnp_TxnRef
    }
}
