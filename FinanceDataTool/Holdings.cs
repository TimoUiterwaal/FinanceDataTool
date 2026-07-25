using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

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

            using var context = new FinanceContext();

            if (CheckifHoldingsExists(context,Symbol))

            {

                Console.WriteLine($"Holding for {Symbol} already exists.");
                return;
            }
            else
            {
                await Userstock.UpdateStock(Symbol);

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

                // INSERT INTO Holding (Symbol, Shares, AvgPurchasePrice).
                // StockRecnum is left at 0 so SQLite assigns the next rowid.
                context.Holdings.Add(this);

                await context.SaveChangesAsync();
            }
        }

        public bool CheckifHoldingsExists(FinanceContext context,String Symbol)
        {
            return context.Holdings.Any(h => h.Symbol == this.Symbol);
        }

        public static List<Holding> GetAllUpdatedHoldings()
        {
            using var context = new FinanceContext();

            return context.Holdings.ToList();
        }
    }
}
