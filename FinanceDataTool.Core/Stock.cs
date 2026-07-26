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
    [JsonPropertyName("c")] public decimal? CurrentPrice { get; set; }
    [JsonPropertyName("d")] public decimal? Change { get; set; }
    [JsonPropertyName("dp")] public double? PercentChange { get; set; }
    [JsonPropertyName("h")] public decimal? High { get; set; }
    [JsonPropertyName("l")] public decimal? Low { get; set; }
    [JsonPropertyName("o")] public decimal? Open { get; set; }
    [JsonPropertyName("pc")] public decimal? PreviousClose { get; set; }
    [JsonPropertyName("t")] public long? Timestamp { get; set; }
}


namespace FinanceDataTool.Core
{


    public class Stock
    {
        public required String Symbol { get; set; }
        public decimal? Change { get; set; }
        public double? PercentageChange { get; set; }
        public decimal? HighPrice { get; set; }
        public decimal? LowPrice { get; set; }
        public decimal? OpenPrice { get; set; }
        public decimal? PreviousClose { get; set; }
        public decimal? CurrentPrice { get; set; }
        public long? Timestamp { get; set; }

        //using overload for main menu options which are called without context
        public async Task<bool> UpdateStock(String Symbol)
        {
            using var context = new FinanceContext();
            return await UpdateStock(context, Symbol);
        }

        public async Task<bool> UpdateStock(FinanceContext context, String Symbol)
        {
            this.Symbol = Symbol.ToUpperInvariant();
            //Check if the timestamp in DB needs to be updated
            var existing = await context.Stocks.FindAsync(Symbol);

            object? result = existing?.Timestamp;

            if (result is not null)
            {
                long LastTimestampfromDB = Convert.ToInt64(result);
                long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                if ((now-LastTimestampfromDB < 30))
                {
                    GetDBStockData(context, Symbol);

                    return true;
                }
            }

            try
                {
                    string testing = FinnhubApi.ApiKey;
                    string body = await FinnhubApi.Client.GetStringAsync("quote?symbol=" + Symbol + "&token=" + FinnhubApi.ApiKey);
                    
                    var quote = JsonSerializer.Deserialize<QuoteResponse>(body) ;

                if (quote is null)
                     {
                         return false;
                     }

                    
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
                        return false;
                }

                if (CurrentPrice == 0)
                {
                    return false;
                }

                }
            catch (HttpRequestException)
                {
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

        public void GetDBStockData(FinanceContext context, String Symbol)
        {
            this.Symbol = Symbol.ToUpperInvariant();

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
