using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace YukiSoraShop.Pages
{
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [IgnoreAntiforgeryToken]
    public class ErrorModel : PageModel
    {
        public string? RequestId { get; set; }
        public int? StatusCode { get; set; }
        public string? Message { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

        private readonly ILogger<ErrorModel> _logger;

        public ErrorModel(ILogger<ErrorModel> logger)
        {
            _logger = logger;
        }

        public void OnGet(int? statusCode = null)
        {
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
            StatusCode = statusCode ?? HttpContext.Response?.StatusCode;
            Message = StatusCode switch
            {
                400 => "Bad request.",
                401 => "Unauthorized.",
                403 => "Forbidden.",
                404 => "The page you requested was not found.",
                408 => "The request timed out.",
                500 => "An internal server error occurred.",
                _ => null
            };
            if (StatusCode >= 500)
            {
                _logger.LogError("Error page served. Status={StatusCode}, RequestId={RequestId}", StatusCode, RequestId);
            }
        }
    }

}
