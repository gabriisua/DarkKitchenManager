using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roscoff.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCatalogTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ingredients",
                table: "plates");

            migrationBuilder.AddColumn<int>(
                name: "PackagingCost",
                table: "plates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "allergens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_allergens", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ingredients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EnergyKjPer100g = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    EnergyKcalPer100g = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    FatsPer100g = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    SaturatedFatsPer100g = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    CarbohydratesPer100g = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    SugarsPer100g = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    FibersPer100g = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    ProteinsPer100g = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    SaltPer100g = table.Column<decimal>(type: "decimal(8,2)", nullable: false),
                    CostPer1000g = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    YieldPercentage = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ingredients", x => x.Id);
                });

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

            migrationBuilder.CreateTable(
                name: "plate_ingredients",
                columns: table => new
                {
                    PlateId = table.Column<int>(type: "int", nullable: false),
                    IngredientId = table.Column<int>(type: "int", nullable: false),
                    WeightInGrams = table.Column<decimal>(type: "decimal(8,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plate_ingredients", x => new { x.PlateId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_plate_ingredients_ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_plate_ingredients_plates_PlateId",
                        column: x => x.PlateId,
                        principalTable: "plates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AllergenIngredient_IngredientsId",
                table: "AllergenIngredient",
                column: "IngredientsId");

            migrationBuilder.CreateIndex(
                name: "IX_plate_ingredients_IngredientId",
                table: "plate_ingredients",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AllergenIngredient");

            migrationBuilder.DropTable(
                name: "plate_ingredients");

            migrationBuilder.DropTable(
                name: "allergens");

            migrationBuilder.DropTable(
                name: "ingredients");

            migrationBuilder.DropColumn(
                name: "PackagingCost",
                table: "plates");

            migrationBuilder.AddColumn<string>(
                name: "Ingredients",
                table: "plates",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
