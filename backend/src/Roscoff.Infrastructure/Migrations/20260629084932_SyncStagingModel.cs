using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Roscoff.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncStagingModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsWowPlate",
                table: "plates");

            migrationBuilder.DropColumn(
                name: "IsXlPlate",
                table: "plates");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsWowPlate",
                table: "plates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsXlPlate",
                table: "plates",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
