using Application.Payments.Interfaces;
using Infrastructure.Payments.Options;
using Infrastructure.Payments.Providers;
using Infrastructure.Payments.Providers.VnPay;
using Infrastructure.Payments.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure
{
    public static class PaymentDependencyInjection
    {
        public static IServiceCollection AddPaymentServices(this IServiceCollection services, IConfiguration config)
        {
            services.Configure<VnPayOptions>(config.GetSection("VnPay"));
            services.AddHttpContextAccessor();

            services.AddScoped<IVnPayGateway, VnPayPaymentGateway>();
            services.AddScoped<IPaymentOrchestrator, PaymentOrchestrator>();
            services.AddScoped<IInvoiceService, InvoiceService>();
            services.AddScoped<IPaymentQueryService, PaymentQueryService>();

            return services;
        }
    }
}
