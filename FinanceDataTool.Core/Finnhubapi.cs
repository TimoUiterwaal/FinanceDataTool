using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceDataTool.Core
{
    public static class FinnhubApi
    {
        public static HttpClient Client { get; set; } = new()
        {
            BaseAddress = new Uri("https://finnhub.io/api/v1/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static string? ApiKey { get; set; }
    }
}
