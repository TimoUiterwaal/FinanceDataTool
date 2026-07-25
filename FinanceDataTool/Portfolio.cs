using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FinanceDataTool
{
    internal class Portfolio
    {
        public async Task RunPortfolio()
        {

            while (true)
            {
                //to do, create method for common menu features to clean up code hyphens etc
                Console.WriteLine("Portfolio Menu");
                Console.WriteLine("--------------------------------------------------------------------------------------------------------");
                Console.WriteLine("View Portfolio Holdings - (v)");
                Console.WriteLine("Add to Portfolio Holdings - (a)");
                Console.WriteLine("Exit to main menu - (e)");

                string? input = Console.ReadLine();

                if (string.Equals(input, "exit", StringComparison.OrdinalIgnoreCase) || string.Equals(input, "e", StringComparison.OrdinalIgnoreCase))
                    return;

                else if (string.Equals(input, "Add", StringComparison.OrdinalIgnoreCase) || string.Equals(input, "a", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("Enter stock to add to portfolio");
                    string? holdingSymbolInput = Console.ReadLine();
                    Console.WriteLine(holdingSymbolInput + " Input <-");
                    if (holdingSymbolInput is null)
                    {
                        Console.WriteLine("Invalid Input");
                        continue;
                    }
                    var holding = new Holding { Symbol = holdingSymbolInput.ToUpper() };

                    await holding.CreateHolding(holdingSymbolInput);

                    continue;
                }
                else if (string.Equals(input, "View", StringComparison.OrdinalIgnoreCase) || string.Equals(input, "v", StringComparison.OrdinalIgnoreCase))
                {
                    using var context = new FinanceContext();
                    double? sum = new double();
                    var Holdings = Holding.GetAllUpdatedHoldings(context);

                    foreach (var holding in Holdings)
                    {
                        var CurrentHoldingStock = new Stock() { Symbol = holding.Symbol };
                        await CurrentHoldingStock.UpdateStock(context,CurrentHoldingStock.Symbol);

                        Console.WriteLine($"Symbol: {holding.Symbol}, Shares: {holding.Shares}, Avg Purchase Price: {holding.AvgPurchasePrice}, Current Price: {CurrentHoldingStock.CurrentPrice}");
                        sum = sum + (CurrentHoldingStock.CurrentPrice * holding.Shares);
                    }

                    Console.WriteLine("Current Value of all holdings : "+ sum);
                    Console.WriteLine("--------------------------------------------------------------------------------------------------------");

                    continue;
                }



            }

        }

    }
}
