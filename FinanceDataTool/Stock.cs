using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;


public class QuoteResponse
{
    [JsonPropertyName("c")] public double? CurrentPrice { get; set; }
    [JsonPropertyName("d")] public double? Change { get; set; }
    [JsonPropertyName("dp")] public double? PercentChange { get; set; }
    [JsonPropertyName("h")] public double? High { get; set; }
    [JsonPropertyName("l")] public double? Low { get; set; }
    [JsonPropertyName("o")] public double? Open { get; set; }
    [JsonPropertyName("pc")] public double? PreviousClose { get; set; }
    [JsonPropertyName("t")] public long? Timestamp { get; set; }
}


namespace FinanceDataTool
{


    public class Stock
    {
        public required String Symbol { get; set; }
        public double? Change { get; set; }
        public double? PercentageChange { get; set; }
        public double? HighPrice { get; set; }
        public double? LowPrice { get; set; }
        public double? OpenPrice { get; set; }
        public double? PreviousClose { get; set; }
        public double? CurrentPrice { get; set; }
        public long? Timestamp { get; set; }

        public async Task<bool> UpdateStock(String Symbol)
        {
            //Check if the timestamp in DB needs to be updated
            using var context = new FinanceContext();

            var existing = await context.Stocks.FindAsync(this.Symbol);

            object? result = existing?.Timestamp;

            //Console.WriteLine(result);
            if (result is not null)
            {
                long LastTimestampfromDB = Convert.ToInt64(result);
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if ((now-LastTimestampfromDB < 30))
                {
                    Console.WriteLine("Using Cached data as DB is less than 30 seconds old");

                    GetDBStockData(Symbol);

                    return true;
                }
            }

            try
                {
                    string body = await Program.client.GetStringAsync("quote?symbol=" + Symbol + "&token=" + Program.ApiKey);
                    var quote = JsonSerializer.Deserialize<QuoteResponse>(body);

                    this.CurrentPrice = quote.CurrentPrice ?? 0;
                    this.Change = quote.Change ?? 0;
                    this.PercentageChange = quote.PercentChange ?? 0;
                    this.HighPrice = quote.High ?? 0;
                    this.LowPrice = quote.Low ?? 0;
                    this.OpenPrice = quote.Open ?? 0;
                    this.PreviousClose = quote.PreviousClose ?? 0;
                    this.CurrentPrice = quote.CurrentPrice ?? 0;
                    this.Timestamp = quote.Timestamp;
                //Console.WriteLine("TIMESTAMP FROM API: " + this.Timestamp);
                    if(Timestamp is null)
                {
                    Console.WriteLine("Timestamp is null, this is unexpected and Cached data has not been updated");
                        return false;
                }
                if (CurrentPrice == 0)
                {
                    Console.WriteLine("Current price of " + this.Symbol + " is 0, this is unexpected and cached data has not been updated");
                    Console.WriteLine("Is it possible this ticker does not exist?");
                    return false;
                }

                }
            catch (HttpRequestException ex)
                {
                Console.WriteLine($"Request for '{Symbol}' failed: {(int?)ex.StatusCode} {ex.StatusCode}");

                return false;
                }


            // INSERT OR REPLACE INTO Stocks: add the row if it is new, otherwise copy
            // this object's values onto the tracked row and let EF issue the UPDATE.
            if (existing is null)
            {
                context.Stocks.Add(this);
            }
            else
            {
                context.Entry(existing).CurrentValues.SetValues(this);
            }

            await context.SaveChangesAsync();

            return true;

        }

        public void GetDBStockData(String Symbol)
        {
            this.Symbol = Symbol;

            using var context = new FinanceContext();

            var stock = context.Stocks.Find(this.Symbol);

            if (stock is not null)  // null when no matching row
            {
                this.CurrentPrice = stock.CurrentPrice;
                this.Change = stock.Change;
                this.PercentageChange = stock.PercentageChange;
                this.HighPrice = stock.HighPrice;
                this.LowPrice = stock.LowPrice;
                this.OpenPrice = stock.OpenPrice;
                this.PreviousClose = stock.PreviousClose;
                this.Timestamp = stock.Timestamp;
            }

            return;
        }

    }
}
