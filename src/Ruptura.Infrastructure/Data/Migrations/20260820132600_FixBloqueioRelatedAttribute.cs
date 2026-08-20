using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixBloqueioRelatedAttribute : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000010"),
                column: "DataJson",
                value: "{\"Area\":\"Combate \\u2014 Defesa\",\"RelatedAttribute\":\"Vigor\"}");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000010"),
                column: "DataJson",
                value: "{\"Area\":\"Combate \\u2014 Defesa\",\"RelatedAttribute\":\"Controle\"}");
        }
    }
}
