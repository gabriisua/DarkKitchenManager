using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roscoff.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateAllergenIngredientTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_plate_ingredients_ingredients_IngredientId",
                table: "plate_ingredients");

            migrationBuilder.Sql(@"
    IF OBJECT_ID('AllergenIngredient', 'U') IS NOT NULL
    DROP TABLE [AllergenIngredient];
");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightInGrams",
                table: "plate_ingredients",
                type: "decimal(10,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(8,2)");

            migrationBuilder.CreateTable(
                name: "ingredient_allergens",
                columns: table => new
                {
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    AllergenId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingredient_allergens", x => new { x.IngredientId, x.AllergenId });
                    table.ForeignKey(
                        name: "FK_ingredient_allergens_allergens_AllergenId",
                        column: x => x.AllergenId,
                        principalTable: "allergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ingredient_allergens_ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ingredient_allergens_AllergenId",
                table: "ingredient_allergens",
                column: "AllergenId");

            migrationBuilder.AddForeignKey(
                name: "FK_plate_ingredients_ingredients_IngredientId",
                table: "plate_ingredients",
                column: "IngredientId",
                principalTable: "ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_plate_ingredients_ingredients_IngredientId",
                table: "plate_ingredients");

            migrationBuilder.DropTable(
                name: "ingredient_allergens");

            migrationBuilder.AlterColumn<decimal>(
                name: "WeightInGrams",
                table: "plate_ingredients",
                type: "decimal(8,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(10,2)");

            migrationBuilder.CreateTable(
                name: "AllergenIngredient",
                columns: table => new
                {
                    AllergensId = table.Column<int>(type: "int", nullable: false),
                    IngredientsId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AllergenIngredient", x => new { x.AllergensId, x.IngredientsId });
                    table.ForeignKey(
                        name: "FK_AllergenIngredient_allergens_AllergensId",
                        column: x => x.AllergensId,
                        principalTable: "allergens",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AllergenIngredient_ingredients_IngredientsId",
                        column: x => x.IngredientsId,
                        principalTable: "ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllergenIngredient_IngredientsId",
                table: "AllergenIngredient",
                column: "IngredientsId");

            migrationBuilder.AddForeignKey(
                name: "FK_plate_ingredients_ingredients_IngredientId",
                table: "plate_ingredients",
                column: "IngredientId",
                principalTable: "ingredients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
