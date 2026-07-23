using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roscoff.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerReference",
                table: "orders",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "OrderSequence",
                table: "orders",
                type: "int",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:Identity", "1000, 1");

            migrationBuilder.AddColumn<string>(
                name: "OrderNumber",
                table: "orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                computedColumnSql: "'ORD-' + CAST([OrderSequence] AS VARCHAR(20))",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderNumber",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "CustomerReference",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "OrderSequence",
                table: "orders");
        }
    }
}
