using Application.Payments.Interfaces;
using Infrastructure;
using Infrastructure.Payments.Options;
using Infrastructure.Payments.Providers.VnPay;
using Serilog;
using YukiSoraShop.Filters;
using YukiSoraShop.Hubs;

namespace YukiSoraShop
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                        .WriteTo.Console()
                        .WriteTo.File("Logs/logBootstrap-.txt", rollingInterval: RollingInterval.Day)
                        .CreateBootstrapLogger();
            try
            {
                var builder = WebApplication.CreateBuilder(args);

                builder.Host.UseSerilog((ctx, services, cfg) =>
                    cfg.ReadFrom.Configuration(ctx.Configuration)
                       .ReadFrom.Services(services)
                       .Enrich.FromLogContext());

                builder.Services
                    .AddRazorPages()
                    .AddMvcOptions(o => { o.Filters.Add<GlobalExceptionPageFilter>(); });

                // Configure Cookie Authentication
                builder.Services.AddAuthentication("CookieAuth")
                    .AddCookie("CookieAuth", options =>
                    {
                        options.Cookie.Name = ".YukiSora.Auth";
                        options.Cookie.HttpOnly = true;
                        options.Cookie.SameSite = SameSiteMode.Lax;
                        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

                        options.LoginPath = "/Auth/Login";
                        options.LogoutPath = "/Auth/Logout";
                        options.AccessDeniedPath = "/Auth/Login";
                        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                        options.SlidingExpiration = true;
                    })
                    // Add Google OAuth Authentication
                    .AddGoogle("Google", options =>
                    {
                        options.ClientId = builder.Configuration["Google:ClientId"] ?? "";
                        options.ClientSecret = builder.Configuration["Google:ClientSecret"] ?? "";
                        options.SignInScheme = "CookieAuth";
                        options.SaveTokens = true;

                        // Request additional scopes if needed
                        options.Scope.Add("profile");
                        options.Scope.Add("email");

                        options.CallbackPath = "/signin-google";
                    });

                builder.Services.AddAuthorization();

                builder.Services.AddInfrastructureServices(builder.Configuration);
                builder.Services.AddPaymentServices(builder.Configuration);
                builder.Services.AddSignalR();

                builder.Services.AddDistributedMemoryCache();
                builder.Services.AddMemoryCache();
                builder.Services.AddSession(options =>
                {
                    options.IdleTimeout = TimeSpan.FromMinutes(30);
                    options.Cookie.Name = ".YukiSora.Session";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.IsEssential = true;
                    options.Cookie.SameSite = SameSiteMode.Lax;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                });

                // Register VNPay options from configuration using correct section name "VnPay"
                builder.Services.Configure<VnPayOptions>(builder.Configuration.GetSection("VnPay"));
                builder.Services.AddSingleton<IVnPayGateway, VnPayPaymentGateway>();

                // ensure IHttpContextAccessor exists (for IP helpers)
                builder.Services.AddHttpContextAccessor();

                var app = builder.Build();

                if (!app.Environment.IsDevelopment())
                {
                    app.UseExceptionHandler("/Error");
                    app.UseHsts();
                }

                app.UseSerilogRequestLogging();

                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();
                // Razor Pages-style error handling: use /Error for exceptions and status codes
                app.UseStatusCodePagesWithReExecute("/Error", "?statusCode={0}");
                app.UseSession();

                app.UseAuthentication();
                app.UseAuthorization();

                app.MapRazorPages();
                app.MapHub<AdminDashboardHub>("/hubs/adminDashboard");
                app.MapGet("/", () => Results.Redirect("/Home"));

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application failed to start");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
    }
}