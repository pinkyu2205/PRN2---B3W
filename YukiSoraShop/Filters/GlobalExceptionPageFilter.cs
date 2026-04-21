using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;

namespace YukiSoraShop.Filters
{
    // Global Razor Pages filter to catch exceptions and redirect to /Error with status code
    public sealed class GlobalExceptionPageFilter : IAsyncPageFilter
    {
        private readonly ILogger<GlobalExceptionPageFilter> _logger;

        public GlobalExceptionPageFilter(ILogger<GlobalExceptionPageFilter> logger)
        {
            _logger = logger;
        }

        public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
            => Task.CompletedTask;

        public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
        {
            try
            {
                await next();
            }
            catch (Exception ex)
            {
                var status = MapStatusCode(ex);
                var requestId = context.HttpContext.TraceIdentifier;
                _logger.LogError(ex, "Unhandled page exception. Status={Status} RequestId={RequestId} Path={Path}", status, requestId, context.HttpContext.Request.Path);

                // Clear then redirect to /Error with status code
                context.HttpContext.Response.Clear();
                context.HttpContext.Response.StatusCode = status;
                context.Result = new RedirectToPageResult("/Error", new { statusCode = status });
            }
        }

        private static int MapStatusCode(Exception ex)
        {
            return ex switch
            {
                UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
                KeyNotFoundException => StatusCodes.Status404NotFound,
                OperationCanceledException => 408,
                DbUpdateException => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status500InternalServerError
            };
        }
    }
}

