using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteCodeUsedAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UsedAt",
                table: "InviteCodes",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsedAt",
                table: "InviteCodes");
        }
    }
}
