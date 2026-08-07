using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildBuildingUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GuildBuildings_GuildSheetId",
                table: "GuildBuildings");

            migrationBuilder.CreateIndex(
                name: "ux_guild_buildings_sheet_installation",
                table: "GuildBuildings",
                columns: new[] { "GuildSheetId", "CatalogEntryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ux_guild_buildings_sheet_installation",
                table: "GuildBuildings");

            migrationBuilder.CreateIndex(
                name: "IX_GuildBuildings_GuildSheetId",
                table: "GuildBuildings",
                column: "GuildSheetId");
        }
    }
}
