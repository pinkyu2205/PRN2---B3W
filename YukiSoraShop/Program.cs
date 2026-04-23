using Infrastructure;
using Serilog;
using YukiSoraShop.Filters;
using YukiSoraShop.Hubs;
using YukiSoraShop.Extensions;

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
                builder.Services.AddScoped<Application.Services.Interfaces.IRealtimeService, YukiSoraShop.Services.RealtimeService>();

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
                app.MapHub<NotificationHub>("/hubs/notification");
                app.MapHub<CatalogHub>("/hubs/catalog");
                
                app.MapSePayEndpoints();

                app.MapGet("/", () => Results.Redirect("/Customer/Catalog"));

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
