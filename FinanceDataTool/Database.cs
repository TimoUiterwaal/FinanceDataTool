using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FinanceDataTool
{
    internal class Database
    {
        private const int CurrentDbVersion = 0;  // bump this each time you add a DBupdate

        public static void InitializeDB()
        {
            using var context = new FinanceContext();

            Console.Write("Initializing DB...");

            // Applies any migrations the database hasn't run yet, creating the schema
            // on first launch. Replaces the CREATE TABLE IF NOT EXISTS statements.
            context.Database.Migrate();

            Console.WriteLine("Initializing DB Completed...");

        }
    }
}
