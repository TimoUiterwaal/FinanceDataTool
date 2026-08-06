using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinanceDataTool.Migrations
{
    /// <inheritdoc />
    public partial class AddLastFetchedUnix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "LastFetchedUnix",
                table: "Stocks",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LastFetchedUnix",
                table: "Stocks");
        }
    }
}
