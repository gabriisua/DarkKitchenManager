using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roscoff.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePlatesTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EanCode",
                table: "plates",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MicrowaveMinutes",
                table: "plates",
                type: "decimal(4,1)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MicrowaveWattage",
                table: "plates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreparationInstructions",
                table: "plates",
                type: "nvarchar(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EanCode",
                table: "plates");

            migrationBuilder.DropColumn(
                name: "MicrowaveMinutes",
                table: "plates");

            migrationBuilder.DropColumn(
                name: "MicrowaveWattage",
                table: "plates");

            migrationBuilder.DropColumn(
                name: "PreparationInstructions",
                table: "plates");
        }
    }
}
