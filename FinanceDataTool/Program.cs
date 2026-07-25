using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace FinanceDataTool
{
    internal class Program
    {
        public static readonly HttpClient client = new()
        {
            BaseAddress = new Uri("https://finnhub.io/api/v1/"),
            Timeout = TimeSpan.FromSeconds(30)
        };

        public static IConfiguration Configuration = new ConfigurationBuilder().AddUserSecrets<Program>().Build();

        public static readonly string ApiKey = Configuration["Finnhub:ApiKey"];

        public class MarketStatusResponse
        {
            [JsonPropertyName("exchange")] public string exchange { get; set; }
            [JsonPropertyName("holiday")] public string holiday { get; set; }
            [JsonPropertyName("isOpen")] public bool isOpen { get; set; }
            [JsonPropertyName("session")] public string session { get; set; }
            [JsonPropertyName("t")] public long Timestamp { get; set; }
            [JsonPropertyName("timezone")] public string timezone { get; set; }

        }

        static async Task Main(string[] args)
        {

            Database.InitializeDB();
            CheckForSecret();
            MarketStatusResponse Marketstatus = await WithSpinner(GetMarketStatus());
            Console.WriteLine("Welcome to the Finance Data Tool");
            Console.WriteLine("Market Status: " + (Marketstatus.isOpen ? "Open" : "Closed"));

            var SPYIntroticker = new Stock
            {
                Symbol = "SPY"

            };

            await WithSpinner(SPYIntroticker.UpdateStock(SPYIntroticker.Symbol));

            Console.WriteLine("S&P 500 now: " + SPYIntroticker.CurrentPrice + " | " + SPYIntroticker.PercentageChange + "%");

            while (true)
            {
                Console.WriteLine("Menu options - Stock (S) - Portfolio (P) - exit");

                string input = Console.ReadLine();


                if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase))
                    break;

                else if (string.Equals(input, "portfolio", StringComparison.OrdinalIgnoreCase) || string.Equals(input, "p", StringComparison.OrdinalIgnoreCase))
                {
                    var currentportfolio = new Portfolio();

                    await currentportfolio.RunPortfolio();
                    continue;
                }

                else if (string.Equals(input, "stock", StringComparison.OrdinalIgnoreCase) || string.Equals(input, "s", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Enter Ticker (or 'exit' to quit): ");
                    string stocksymbolinput = Console.ReadLine();

                    if (stocksymbolinput is null)
                    {
                        Console.WriteLine("Invalid Input");
                        continue;
                    }

                    var currentticker = new Stock
                    {
                        Symbol = stocksymbolinput.ToUpper()
                    };
                    //Console.WriteLine(currentticker.Symbol);
                    Console.WriteLine("--------------------------------------------------------------------------------------------------------");

                    await WithSpinner(currentticker.UpdateStock(currentticker.Symbol));

                    if (currentticker.CurrentPrice != 0)
                    {
                        Console.WriteLine(currentticker.Symbol + " now: " + currentticker.CurrentPrice + " | " + currentticker.PercentageChange + "%");
                    }
                    continue;
                }

                if (input is null)
                {
                    Console.WriteLine("No value Input");
                    continue;
                }
                else
                {
                    Console.WriteLine("Invalid Input");
                    continue;
                }
            }
        }

        static async Task<MarketStatusResponse> GetMarketStatus()
        {

            try
            { 
                string body = await client.GetStringAsync("stock/market-status?exchange=US&token=" + ApiKey);
                var MarketStatusResponse = JsonSerializer.Deserialize<MarketStatusResponse>(body);
                using var context = new FinanceContext();

                var systemInfo = context.SystemInfo.Find(1L);
                if (systemInfo is not null)
                {
                    systemInfo.LastTimestamp = MarketStatusResponse.Timestamp;
                    context.SaveChanges();
                }
                return MarketStatusResponse;
            }
            catch (Exception)
            {

                throw;

            }

        }
        static void CheckForSecret()
        {
            if (Program.ApiKey is null)
            {
                Console.WriteLine("API Key not found. Please set the Finnhub:ApiKey secret.");
                Console.WriteLine("dotnet user-secrets set \"Finnhub:ApiKey\" \"ENTER SECRET HERE\"");
                Environment.Exit(1);


            }
        }
        static async Task<T> WithSpinner<T>( Task<T> task)
        {
            string[] frames = { "!", "*"};
            int i = 0;

            while (!task.IsCompleted)
            {
                Console.Write($"\r{frames[i++ % frames.Length]}");
                await Task.Delay(120);
            }

            Console.Write($"\r"); // clear the line
            return await task; // re-awaiting a completed task just returns its result (or rethrows its exception)
        }
    }


}

