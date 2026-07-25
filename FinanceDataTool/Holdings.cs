using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FinanceDataTool
{
    public class Holding
    {
        public long StockRecnum { get; set; }
        public required string Symbol { get; set; }
        public decimal Shares { get; set; }
        public decimal AvgPurchasePrice { get; set; }
        public decimal? CurrentPrice { get; set; }
        public async Task CreateHolding( String Symbol)
        {
            using var context = new FinanceContext();
            await CreateHolding (context, Symbol);

        }
        public async Task CreateHolding(FinanceContext context, String Symbol)
        {
            decimal usershares;
            decimal userpurchaseprice;
            Stock Userstock = new Stock { Symbol = Symbol };

            if (CheckifHoldingsExists(context,Symbol))

            {

                Console.WriteLine($"Holding for {Symbol} already exists.");
                return;
            }
            else
            {
                if (!await Userstock.UpdateStock(context, Symbol))
                {
                    return;
                }

                while (true)
                {
                    Console.WriteLine("How many shares of this stock were purchased?");
                    if (decimal.TryParse(Console.ReadLine(), out usershares))
                        break;
                    Console.WriteLine("Invalid input — please enter a number.");
                }
                this.Shares = usershares;

                while (true)
                {
                    Console.WriteLine("What was the purchase price of this stock?");
                    if (decimal.TryParse(Console.ReadLine(), out userpurchaseprice))
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

        public void UpdateHolding(String Symbol)
        {
            using var context = new FinanceContext();
            UpdateHolding(context, Symbol);
        }

        public void UpdateHolding(FinanceContext context, String Symbol)
        {
            var holdingToUpdate = context.Holdings.FirstOrDefault(h => h.Symbol == Symbol);

            Console.WriteLine($"Current Shares: {holdingToUpdate.Shares}");

            while (true)
            {
                Console.WriteLine("How many shares are currently owned?");
                string? input = Console.ReadLine();

                if (decimal.TryParse(input, out decimal result))
                {
                    holdingToUpdate.Shares = result; // valid number typed → use it
                    break;
                }
                    Console.WriteLine("Unable to parse input, enter new input"); // invalid/blank → keep what's there
            }

            while (true)
            {
                Console.WriteLine("What was the purchase price of the shares that you currently own?");
                string? input = Console.ReadLine();

                if (decimal.TryParse(input, out decimal result))
                {
                    holdingToUpdate.AvgPurchasePrice = result; // valid number typed → use it
                    break;
                }
                Console.WriteLine("Unable to parse input, enter new input"); // invalid/blank → keep what's there
            }

            context.SaveChanges();

        }


        public void AddtoHolding(String Symbol)
        {
            using var context = new FinanceContext();
            AddtoHolding(context, Symbol);
        }

        public void AddtoHolding(FinanceContext context, String Symbol)
        {
            var holdingToUpdate = context.Holdings.FirstOrDefault(h => h.Symbol == Symbol);
            decimal ashares;
            decimal aavgprice;

            Console.WriteLine($"Current Shares: {holdingToUpdate.Shares}");
            Console.WriteLine($"Current Avg Purchase Price: {holdingToUpdate.AvgPurchasePrice}");


            while (true)
            {
                Console.WriteLine("How many shares are you adding?");
                string? input = Console.ReadLine();

                if (decimal.TryParse(input, out decimal result))
                {
                    ashares = result; // valid number typed → use it
                    break;
                }
                Console.WriteLine("Unable to parse input, enter new input"); // invalid/blank → keep what's there
            }

            while (true)
            {
                Console.WriteLine("What was the purchase price of these shares?");
                string? input = Console.ReadLine();

                if (decimal.TryParse(input, out decimal result))
                {
                    aavgprice = result; // valid number typed → use it
                    break;
                }
                Console.WriteLine("Unable to parse input, enter new input"); // invalid/blank → keep what's there
            }

            holdingToUpdate.AvgPurchasePrice = ((holdingToUpdate.Shares * holdingToUpdate.AvgPurchasePrice) + (aavgprice * ashares))/(ashares+holdingToUpdate.Shares);
            holdingToUpdate.Shares = ashares + holdingToUpdate.Shares;

            context.SaveChanges();

        }


        public bool CheckifHoldingsExists(FinanceContext context,String Symbol)
        {
            return context.Holdings.Any(h => h.Symbol == this.Symbol);
        }

        public static List<Holding> GetAllUpdatedHoldings(FinanceContext context)
        {

            return context.Holdings.ToList();
        }
    }
}
