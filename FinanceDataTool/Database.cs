using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace FinanceDataTool
{
    internal class Database
    {
        public static SqliteConnection CreateConnection()
        {
            var connection = new SqliteConnection("Data Source=stocks.db");
            connection.Open();

            var pragma = connection.CreateCommand();
            pragma.CommandText = "PRAGMA foreign_keys = ON;";
            pragma.ExecuteNonQuery();

            return connection;
        }

    }
}
