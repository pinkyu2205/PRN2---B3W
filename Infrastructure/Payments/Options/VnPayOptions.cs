using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Payments.Options
{
    public class VnPayOptions
    {
        public string vnp_TmnCode { get; set; } = default!;
        public string vnp_HashSecret { get; set; } = default!;
        public string vnp_BaseUrl { get; set; } = default!;
        public string vnp_Command { get; set; } = "pay";
        public string vnp_CurrCode { get; set; } = "VND";
        public string vnp_Locale { get; set; } = "vn";
        public string vnp_ReturnUrl { get; set; } = default!;
        public string TimeZoneId { get; set; } = "SE Asia Standard Time";
        public int ExpireMinutes { get; set; } = 15;
    }
}
