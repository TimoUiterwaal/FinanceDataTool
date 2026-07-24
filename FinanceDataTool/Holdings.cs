using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceDataTool
{
    internal class Holding
    {
        public long StockRecnum { get; set; }
        public required string Symbol { get; set; }
        public double Shares { get; set; }
        public double AvgPurchasePrice { get; set; }

        public double? CurrentPrice { get; set; }

        public async Task CreateHolding(String Symbol)
        {
            double usershares;
            double userpurchaseprice;
            Stock Userstock = new Stock { Symbol = Symbol };

            using var connection = Database.CreateConnection();

            if (CheckifHoldingsExists(Symbol))

            {

                Console.WriteLine($"Holding for {Symbol} already exists.");
                return;
            }
            else
            {
                await Userstock.UpdateStock(Symbol);

                var insert = connection.CreateCommand();
                insert.CommandText = "INSERT INTO Holding (Symbol, Shares, AvgPurchasePrice) VALUES ($symbol, $shares, $avgPurchasePrice)";
                insert.Parameters.AddWithValue("$symbol", this.Symbol);

                while (true)
                {
                    Console.WriteLine("How many shares of this stock were purchased?");
                    if (double.TryParse(Console.ReadLine(), out usershares))
                        break;
                    Console.WriteLine("Invalid input — please enter a number.");
                }
                this.Shares = usershares;

                while (true)
                {
                    Console.WriteLine("What was the purchase price of this stock?");
                    if (double.TryParse(Console.ReadLine(), out userpurchaseprice))
                        break;
                    Console.WriteLine("Invalid input — please enter a number.");
                }
                this.AvgPurchasePrice = userpurchaseprice;

                insert.Parameters.AddWithValue("$shares", this.Shares);
                insert.Parameters.AddWithValue("$avgPurchasePrice", this.AvgPurchasePrice);

                insert.ExecuteNonQuery();
            }
        }

        public bool CheckifHoldingsExists(String Symbol)
        {
            using var connection = Database.CreateConnection();
            var select = connection.CreateCommand();
            select.CommandText = "SELECT StockRecnum FROM Holding WHERE Symbol = $symbol";
            select.Parameters.AddWithValue("$symbol", this.Symbol);
            object result = select.ExecuteScalar();
            return result is not null;
        }

        public static List<Holding> GetAllUpdatedHoldings()
        {
            var holdings = new List<Holding>();

            using var connection = Database.CreateConnection();
            var select = connection.CreateCommand();
            select.CommandText = "SELECT StockRecnum, Symbol, Shares, AvgPurchasePrice FROM Holding";

            using var reader = select.ExecuteReader();
            while (reader.Read())
            {
                var holding = new Holding
                {
                    Symbol = reader.GetString(reader.GetOrdinal("Symbol"))
                };
                holding.StockRecnum = reader.GetInt64(reader.GetOrdinal("StockRecnum"));
                holding.Shares = reader.GetDouble(reader.GetOrdinal("Shares"));
                holding.AvgPurchasePrice = reader.GetDouble(reader.GetOrdinal("AvgPurchasePrice"));

                holdings.Add(holding);
            }

            return holdings;
        }
    }
}