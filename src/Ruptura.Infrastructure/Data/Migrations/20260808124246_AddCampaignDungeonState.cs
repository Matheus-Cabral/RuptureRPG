using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCampaignDungeonState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentFloor",
                table: "Campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<string>(
                name: "FloorName",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FloorState",
                table: "Campaigns",
                type: "text",
                nullable: false,
                defaultValue: "Inexplorado");

            migrationBuilder.AddColumn<int>(
                name: "Pressure",
                table: "Campaigns",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentFloor",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "FloorName",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "FloorState",
                table: "Campaigns");

            migrationBuilder.DropColumn(
                name: "Pressure",
                table: "Campaigns");
        }
    }
}
