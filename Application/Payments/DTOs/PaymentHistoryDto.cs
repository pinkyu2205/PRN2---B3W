using Domain.Entities;
using Domain.Enums;

namespace Application.Payments.DTOs
{
    /// <summary>
    /// DTO cho l?ch s? thanh toán c?a user
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
            PaymentStatus.Pending => "?ang ch?",
            PaymentStatus.Paid => "?ã thanh toán",
            PaymentStatus.Canceled => "?ã h?y",
            PaymentStatus.Refunded => "?ã hoàn ti?n",
            _ => "Không xác ??nh"
        };
        public string? TransactionRef { get; set; }
        public DateTime CreatedAt { get; set; }
        public string CreatedAtDisplay => CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
        
        // Order info
        public decimal OrderTotal { get; set; }
        public string OrderStatus { get; set; } = string.Empty;
    }
}
