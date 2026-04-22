using Domain.Enums;

namespace Application.Payments.DTOs
{
    /// <summary>
    /// DTO cho lịch sử thanh toán của người dùng.
    /// </summary>
    public class PaymentHistoryDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "VND";
        public PaymentStatus Status { get; set; }
        public string StatusDisplay => Status switch
        {
            PaymentStatus.Pending => "Đang chờ",
            PaymentStatus.Paid => "Đã thanh toán",
            PaymentStatus.Canceled => "Đã hủy",
            PaymentStatus.Refunded => "Đã hoàn tiền",
            _ => "Không xác định"
        };
        public string? TransactionRef { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");

        public decimal OrderTotal { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
    }
}
