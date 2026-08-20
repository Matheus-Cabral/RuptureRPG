using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCatalogEntryIsPublic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "CatalogEntries",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000013"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000014"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000015"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000016"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000017"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000018"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000019"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000020"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000007"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000008"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000009"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000010"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000011"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000012"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000013"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000014"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000015"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000016"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000017"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000018"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000019"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000020"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000007"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000008"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000009"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000010"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000007"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000008"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000009"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000010"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000011"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000012"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000013"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000014"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000015"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000016"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000017"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000018"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000019"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000020"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000007"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000008"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000009"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000010"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000011"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000012"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000013"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000014"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000015"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000016"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000017"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000018"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000019"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000020"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000021"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000022"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000023"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000024"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000025"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000026"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000027"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000028"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000029"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000030"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000031"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000032"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000033"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000034"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000035"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000036"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000037"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000038"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000039"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000040"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000041"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000042"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000043"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000044"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000045"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000046"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000047"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000048"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000049"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000050"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000051"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000052"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000053"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000054"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000055"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000056"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000057"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000058"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000059"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000007"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000008"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000009"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000010"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000011"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000012"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000013"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000014"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000015"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000016"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000017"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000018"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000019"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000020"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000021"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000022"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000023"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000024"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000007"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000008"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000009"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000010"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000011"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000012"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000007"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000008"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000009"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000010"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000011"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000012"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000013"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000014"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000015"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000016"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000017"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000018"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000019"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000020"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000001"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000002"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000003"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000004"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000005"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000006"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000007"),
                column: "IsPublic",
                value: true);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000008"),
                column: "IsPublic",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "CatalogEntries");
        }
    }
}
