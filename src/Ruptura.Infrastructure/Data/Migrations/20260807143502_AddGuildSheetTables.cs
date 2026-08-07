using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildSheetTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GuildMemberships");

            migrationBuilder.AddColumn<Guid>(
                name: "CampaignId",
                table: "GuildSheets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "GuildSheets",
                type: "bytea",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.CreateTable(
                name: "CraftingOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    ItemName = table.Column<string>(type: "text", nullable: false),
                    Quality = table.Column<string>(type: "text", nullable: false),
                    ProgressDays = table.Column<int>(type: "integer", nullable: false),
                    RequiredDays = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CraftingOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CraftingOrders_GuildSheets_GuildSheetId",
                        column: x => x.GuildSheetId,
                        principalTable: "GuildSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Expeditions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Participants = table.Column<string>(type: "text", nullable: false),
                    Objective = table.Column<string>(type: "text", nullable: false),
                    Result = table.Column<string>(type: "text", nullable: false),
                    Losses = table.Column<string>(type: "text", nullable: false),
                    ResourcesGained = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expeditions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expeditions_GuildSheets_GuildSheetId",
                        column: x => x.GuildSheetId,
                        principalTable: "GuildSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildBuildings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    CatalogEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildBuildings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildBuildings_GuildSheets_GuildSheetId",
                        column: x => x.GuildSheetId,
                        principalTable: "GuildSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GuildStaff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Kind = table.Column<string>(type: "text", nullable: false),
                    TypeOrRanking = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    DailySalary = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    Efficiency = table.Column<int>(type: "integer", nullable: true),
                    Morale = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildStaff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildStaff_GuildSheets_GuildSheetId",
                        column: x => x.GuildSheetId,
                        principalTable: "GuildSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResearchProjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ResearchType = table.Column<string>(type: "text", nullable: false),
                    Complexity = table.Column<string>(type: "text", nullable: false),
                    Stage = table.Column<string>(type: "text", nullable: false),
                    ProgressDays = table.Column<int>(type: "integer", nullable: false),
                    RequiredDays = table.Column<int>(type: "integer", nullable: false),
                    Researchers = table.Column<int>(type: "integer", nullable: false),
                    Points = table.Column<int>(type: "integer", nullable: false),
                    IsComplete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResearchProjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ResearchProjects_GuildSheets_GuildSheetId",
                        column: x => x.GuildSheetId,
                        principalTable: "GuildSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ux_guild_sheets_campaign",
                table: "GuildSheets",
                column: "CampaignId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CraftingOrders_GuildSheetId",
                table: "CraftingOrders",
                column: "GuildSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_Expeditions_GuildSheetId",
                table: "Expeditions",
                column: "GuildSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildBuildings_GuildSheetId",
                table: "GuildBuildings",
                column: "GuildSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_GuildStaff_GuildSheetId",
                table: "GuildStaff",
                column: "GuildSheetId");

            migrationBuilder.CreateIndex(
                name: "IX_ResearchProjects_GuildSheetId",
                table: "ResearchProjects",
                column: "GuildSheetId");

            migrationBuilder.AddForeignKey(
                name: "FK_GuildSheets_Campaigns_CampaignId",
                table: "GuildSheets",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GuildSheets_Campaigns_CampaignId",
                table: "GuildSheets");

            migrationBuilder.DropTable(
                name: "CraftingOrders");

            migrationBuilder.DropTable(
                name: "Expeditions");

            migrationBuilder.DropTable(
                name: "GuildBuildings");

            migrationBuilder.DropTable(
                name: "GuildStaff");

            migrationBuilder.DropTable(
                name: "ResearchProjects");

            migrationBuilder.DropIndex(
                name: "ux_guild_sheets_campaign",
                table: "GuildSheets");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "GuildSheets");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "GuildSheets");

            migrationBuilder.CreateTable(
                name: "GuildMemberships",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    GuildSheetId = table.Column<Guid>(type: "uuid", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuildMemberships", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GuildMemberships_GuildSheets_GuildSheetId",
                        column: x => x.GuildSheetId,
                        principalTable: "GuildSheets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GuildMemberships_GuildSheetId",
                table: "GuildMemberships",
                column: "GuildSheetId");
        }
    }
}
