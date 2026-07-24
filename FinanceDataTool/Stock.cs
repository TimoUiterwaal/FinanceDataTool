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
using Microsoft.Data.Sqlite;


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
            using var connection = Database.CreateConnection();

            var select = connection.CreateCommand();
            select.CommandText = "SELECT Timestamp FROM Stocks WHERE Symbol = $symbol";
            select.Parameters.AddWithValue("$symbol", this.Symbol);

            object result = select.ExecuteScalar();

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


            var update = connection.CreateCommand();
            update.CommandText = @"INSERT OR REPLACE INTO Stocks
                  (Symbol, CurrentPrice, Change, PercentageChange, HighPrice, LowPrice, OpenPrice, PreviousClose, Timestamp)
                VALUES
                  ($symbol, $currentPrice, $change, $percentChange, $high, $low, $open, $previousClose, $timestamp)";

            update.Parameters.AddWithValue("$symbol", this.Symbol);
            update.Parameters.AddWithValue("$currentPrice", this.CurrentPrice);
            update.Parameters.AddWithValue("$change", this.Change);
            update.Parameters.AddWithValue("$percentChange", this.PercentageChange);
            update.Parameters.AddWithValue("$high", this.HighPrice);
            update.Parameters.AddWithValue("$low", this.LowPrice);
            update.Parameters.AddWithValue("$open", this.OpenPrice);
            update.Parameters.AddWithValue("$previousClose", this.PreviousClose);
            update.Parameters.AddWithValue("$timestamp", this.Timestamp);

            update.ExecuteNonQuery();

            return true;

        }

        public void GetDBStockData(String Symbol)
        {
            this.Symbol = Symbol;

            using var connection = Database.CreateConnection();
            var selectnew = connection.CreateCommand();

            selectnew.CommandText = "SELECT * FROM Stocks WHERE Symbol = $symbol";
            selectnew.Parameters.AddWithValue("$symbol", this.Symbol);

            using var readernew = selectnew.ExecuteReader();

            if (readernew.Read())  // moves to the first row; false if no matching row
            {
                this.CurrentPrice = readernew.IsDBNull(readernew.GetOrdinal("CurrentPrice")) ? null : readernew.GetDouble(readernew.GetOrdinal("CurrentPrice"));
                this.Change = readernew.IsDBNull(readernew.GetOrdinal("Change")) ? null : readernew.GetDouble(readernew.GetOrdinal("Change"));
                this.PercentageChange = readernew.IsDBNull(readernew.GetOrdinal("PercentageChange")) ? null : readernew.GetDouble(readernew.GetOrdinal("PercentageChange"));
                this.HighPrice = readernew.IsDBNull(readernew.GetOrdinal("HighPrice")) ? null : readernew.GetDouble(readernew.GetOrdinal("HighPrice"));
                this.LowPrice = readernew.IsDBNull(readernew.GetOrdinal("LowPrice")) ? null : readernew.GetDouble(readernew.GetOrdinal("LowPrice"));
                this.OpenPrice = readernew.IsDBNull(readernew.GetOrdinal("OpenPrice")) ? null : readernew.GetDouble(readernew.GetOrdinal("OpenPrice"));
                this.PreviousClose = readernew.IsDBNull(readernew.GetOrdinal("PreviousClose")) ? null : readernew.GetDouble(readernew.GetOrdinal("PreviousClose"));
                this.Timestamp = readernew.IsDBNull(readernew.GetOrdinal("Timestamp")) ? null : readernew.GetInt64(readernew.GetOrdinal("Timestamp"));
            }
            
            return;
        }

    }
}