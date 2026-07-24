using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using static FinanceDataTool.Program;

namespace FinanceDataTool
{
    internal class Database
    {
        private const int CurrentDbVersion = 0;  // bump this each time you add a DBupdate
        public static SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection("Data Source=stocks.db");
            connection.Open();

            var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();

            return connection;
        }
        public static void InitializeDB()
        {
            using var connection = Database.CreateConnection();

            Console.Write("Initializing DB...");

            // Guarantee the System table exists before querying it
            var createSystemTable = connection.CreateCommand();
            createSystemTable.CommandText =
            @"CREATE TABLE IF NOT EXISTS System (
               Id INTEGER PRIMARY KEY,
               DbVersion INTEGER,
               LastUpdated INTEGER,
               LastTimestamp INTEGER
             )";

            createSystemTable.ExecuteNonQuery();


            var select = connection.CreateCommand();
            select.CommandText = @"SELECT DbVersion FROM System";
            object result = select.ExecuteScalar();

            long? dbVersion = result is null ? 0 : Convert.ToInt64(result);
            Console.WriteLine("DB Version: " + dbVersion);

            if (dbVersion is not null) // moves to the first row; false if no matching row
            {
                Console.WriteLine("Existing DB Found");
            }
            else {
                Console.WriteLine("No version found, creating new database schema...");
                CreateSchema();
            }

            if(dbVersion < CurrentDbVersion)
            {
                Console.WriteLine("DB out of date updating");
                //TODO updateDB();
            }
            else
            {
                Console.WriteLine("No  DB update needed");

            }

            Console.WriteLine("Initializing DB Completed...");

        }

        public static void CreateSchema()
        {
            using var connection = Database.CreateConnection();

            var createTable2 = connection.CreateCommand();
            createTable2.CommandText =
            @"CREATE TABLE IF NOT EXISTS System (
                Id INTEGER PRIMARY KEY,
                DbVersion INTEGER,
                LastUpdated INTEGER,
                LastTimestamp INTEGER
            )";

            var update = connection.CreateCommand();
            update.CommandText = @"INSERT OR REPLACE INTO System
                  (Id,DbVersion )
                VALUES
                  (1,0)";
            update.ExecuteNonQuery();

            createTable2.ExecuteNonQuery();

            var createTable = connection.CreateCommand();
            createTable.CommandText =
            @"CREATE TABLE IF NOT EXISTS Stocks (
                Symbol TEXT PRIMARY KEY,
                CurrentPrice REAL,
                Change REAL,
                PercentageChange REAL,
                HighPrice REAL,
                LowPrice REAL,
                OpenPrice REAL,
                PreviousClose REAL,
                Timestamp INTEGER
            )";

            createTable.ExecuteNonQuery();

            var createTable3 = connection.CreateCommand();
            createTable3.CommandText =
            @"CREATE TABLE IF NOT EXISTS Holding (
            StockRecnum INTEGER PRIMARY KEY,
            Symbol TEXT NOT NULL,
            Shares REAL NOT NULL,
            AvgPurchasePrice REAL NOT NULL,
            FOREIGN KEY (Symbol) REFERENCES Stocks(Symbol)
            )";
            createTable3.ExecuteNonQuery();

            
        }
    }
}
