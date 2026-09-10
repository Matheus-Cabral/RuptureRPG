using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddGuildStaffDedication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "DedicatedCharacterSheetId",
                table: "GuildStaff",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DedicatedSkillArea",
                table: "GuildStaff",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DedicatedCharacterSheetId",
                table: "GuildStaff");

            migrationBuilder.DropColumn(
                name: "DedicatedSkillArea",
                table: "GuildStaff");
        }
    }
}
