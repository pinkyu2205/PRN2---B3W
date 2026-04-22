using Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace YukiSoraShop.Extensions
{
    public static class SePayExtensions
    {
        public static void MapSePayEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/sepay/webhook", async (
                [FromHeader(Name = "Authorization")] string authorization,
                [FromBody] SePayWebhookPayload payload,
                IOrderService orderService,
                IConfiguration config,
                ILogger<Program> logger) =>
            {
                try
                {
                    var apiToken = config["SePay:ApiToken"];
                    if (!string.IsNullOrEmpty(apiToken))
                    {
                        if (string.IsNullOrEmpty(authorization) ||
                            (!authorization.Contains($"Apikey {apiToken}") && !authorization.Contains($"Bearer {apiToken}")))
                        {
                            logger.LogWarning("SePay Webhook Authentication Failed");
                            return Results.Unauthorized();
                        }
                    }

                    if (payload.TransferType != "in")
                    {
                        return Results.Ok(new { success = true, message = "Ignored, not an incoming transaction" });
                    }

                    logger.LogInformation($"SePay Webhook Received: Amount: {payload.TransferAmount}, Content: {payload.Content}, Ref: {payload.ReferenceCode}");

                    // Cú pháp chuyển khoản YUKI + OrderId (ví dụ: YUKI105)
                    var match = Regex.Match(payload.Content, @"YUKI(\d+)", RegexOptions.IgnoreCase);
                    if (!match.Success)
                    {
                        return Results.Ok(new { success = true, message = "No matching YUKI order code found inside content" });
                    }

                    if (!int.TryParse(match.Groups[1].Value, out int orderId))
                    {
                        return Results.Ok(new { success = true, message = "Invalid Order ID format" });
                    }

                    var order = await orderService.GetOrderByIdAsync(orderId);

                    if (order == null || order.Status == "PAID" || order.Status == "Completed")
                    {
                        logger.LogWarning($"Order {orderId} not found or already processed.");
                        return Results.Ok(new { success = true, message = "Order not found or already paid" });
                    }

                    if (payload.TransferAmount < order.GrandTotal)
                    {
                        logger.LogWarning($"Insufficient amount. Expected {order.GrandTotal}, got {payload.TransferAmount}");
                        return Results.Ok(new { success = true, message = "Amount insufficient" });
                    }

                    // Đánh dấu đã thanh toán
                    await orderService.UpdateOrderStatusAsync(orderId, "PAID");

                    logger.LogInformation($"Successfully processed order {orderId}");
                    return Results.Ok(new { success = true, message = "Webhook processed and order granted" });
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "SePay Webhook Error");
                    return Results.StatusCode(500);
                }
            });
        }
    }

    public class SePayWebhookPayload
    {
        public string TransferType { get; set; } = string.Empty;
        public decimal TransferAmount { get; set; }
        public string Content { get; set; } = string.Empty;
        public string ReferenceCode { get; set; } = string.Empty;
    }
}
