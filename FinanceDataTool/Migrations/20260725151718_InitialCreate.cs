using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceDataTool.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Stocks",
                columns: table => new
                {
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Change = table.Column<double>(type: "REAL", nullable: true),
                    PercentageChange = table.Column<double>(type: "REAL", nullable: true),
                    HighPrice = table.Column<double>(type: "REAL", nullable: true),
                    LowPrice = table.Column<double>(type: "REAL", nullable: true),
                    OpenPrice = table.Column<double>(type: "REAL", nullable: true),
                    PreviousClose = table.Column<double>(type: "REAL", nullable: true),
                    CurrentPrice = table.Column<double>(type: "REAL", nullable: true),
                    Timestamp = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stocks", x => x.Symbol);
                });

            migrationBuilder.CreateTable(
                name: "System",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    DbVersion = table.Column<long>(type: "INTEGER", nullable: true),
                    LastUpdated = table.Column<long>(type: "INTEGER", nullable: true),
                    LastTimestamp = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_System", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holding",
                columns: table => new
                {
                    StockRecnum = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Symbol = table.Column<string>(type: "TEXT", nullable: false),
                    Shares = table.Column<double>(type: "REAL", nullable: false),
                    AvgPurchasePrice = table.Column<double>(type: "REAL", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holding", x => x.StockRecnum);
                    table.ForeignKey(
                        name: "FK_Holding_Stocks_Symbol",
                        column: x => x.Symbol,
                        principalTable: "Stocks",
                        principalColumn: "Symbol",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "System",
                columns: new[] { "Id", "DbVersion", "LastTimestamp", "LastUpdated" },
                values: new object[] { 1L, 0L, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_Holding_Symbol",
                table: "Holding",
                column: "Symbol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Holding");

            migrationBuilder.DropTable(
                name: "System");

            migrationBuilder.DropTable(
                name: "Stocks");
        }
    }
}
