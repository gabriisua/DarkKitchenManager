using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roscoff.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMenuTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CustomerId",
                table: "menus",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_menus_CustomerId",
                table: "menus",
                column: "CustomerId");

            migrationBuilder.AddForeignKey(
                name: "FK_menus_customers_CustomerId",
                table: "menus",
                column: "CustomerId",
                principalTable: "customers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_menus_customers_CustomerId",
                table: "menus");

            migrationBuilder.DropIndex(
                name: "IX_menus_CustomerId",
                table: "menus");

            migrationBuilder.DropColumn(
                name: "CustomerId",
                table: "menus");
        }
    }
}
