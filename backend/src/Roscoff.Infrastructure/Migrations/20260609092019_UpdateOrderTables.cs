using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roscoff.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrderTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DeliveryHubId",
                table: "orders",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_orders_DeliveryHubId",
                table: "orders",
                column: "DeliveryHubId");

            migrationBuilder.AddForeignKey(
                name: "FK_orders_customer_delivery_hubs_DeliveryHubId",
                table: "orders",
                column: "DeliveryHubId",
                principalTable: "customer_delivery_hubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_orders_customer_delivery_hubs_DeliveryHubId",
                table: "orders");

            migrationBuilder.DropIndex(
                name: "IX_orders_DeliveryHubId",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DeliveryHubId",
                table: "orders");
        }
    }
}
