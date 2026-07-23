using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roscoff.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIngredientTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SubIngredients",
                table: "ingredients",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SubIngredients",
                table: "ingredients");
        }
    }
}
