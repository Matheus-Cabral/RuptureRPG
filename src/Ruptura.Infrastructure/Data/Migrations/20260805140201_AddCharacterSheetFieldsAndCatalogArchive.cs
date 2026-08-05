using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCharacterSheetFieldsAndCatalogArchive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CampaignId",
                table: "CharacterSheets",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<bool>(
                name: "IsDead",
                table: "CharacterSheets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRetired",
                table: "CharacterSheets",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PortraitImagePath",
                table: "CharacterSheets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsArchived",
                table: "CatalogEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000001"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000002"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000003"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000004"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000005"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000006"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000007"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000008"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000009"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000010"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000011"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000012"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000013"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000014"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000015"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000016"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000017"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000018"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000019"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("10000000-0000-0000-0000-000000000020"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000001"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000002"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000003"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000004"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000005"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000006"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000007"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000008"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000009"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000010"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000011"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000012"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000013"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000014"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000015"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000016"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000017"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000018"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000019"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("20000000-0000-0000-0000-000000000020"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000006"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000007"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000008"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000009"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000010"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000005"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000006"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000004"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000005"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000006"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000007"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000008"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000009"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000010"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000011"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000012"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000013"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000014"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000015"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000016"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000017"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000018"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000019"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000020"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000004"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000005"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000006"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000007"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000008"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000009"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000010"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000011"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000012"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000013"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000014"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000015"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000016"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000017"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000018"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000019"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000020"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000021"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000022"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000023"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000024"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000025"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000026"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000027"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000028"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000029"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000030"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000031"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000032"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000033"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000034"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000035"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000036"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000037"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000038"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000039"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000040"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000041"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000042"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000043"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000044"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000045"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000046"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000047"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000048"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000049"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000050"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000051"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000052"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000053"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000054"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000055"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000056"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000057"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000058"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000059"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000004"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000005"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000006"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000007"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000008"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000009"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000010"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000011"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000012"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000013"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000014"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000015"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000016"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000017"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000018"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000019"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000020"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000021"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000022"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000023"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000024"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000002"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000003"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000004"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000005"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000006"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000007"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000008"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000009"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000010"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000011"),
                column: "IsArchived",
                value: false);

            migrationBuilder.UpdateData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000012"),
                column: "IsArchived",
                value: false);

            migrationBuilder.CreateIndex(
                name: "ux_character_sheets_owner_campaign_alive",
                table: "CharacterSheets",
                columns: new[] { "OwnerId", "CampaignId" },
                unique: true,
                filter: "NOT \"IsDead\" AND NOT \"IsRetired\"");

            migrationBuilder.CreateIndex(
                name: "IX_CatalogEntries_CampaignId",
                table: "CatalogEntries",
                column: "CampaignId");

            migrationBuilder.AddForeignKey(
                name: "FK_CatalogEntries_Campaigns_CampaignId",
                table: "CatalogEntries",
                column: "CampaignId",
                principalTable: "Campaigns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CatalogEntries_Campaigns_CampaignId",
                table: "CatalogEntries");

            migrationBuilder.DropIndex(
                name: "ux_character_sheets_owner_campaign_alive",
                table: "CharacterSheets");

            migrationBuilder.DropIndex(
                name: "IX_CatalogEntries_CampaignId",
                table: "CatalogEntries");

            migrationBuilder.DropColumn(
                name: "CampaignId",
                table: "CharacterSheets");

            migrationBuilder.DropColumn(
                name: "IsDead",
                table: "CharacterSheets");

            migrationBuilder.DropColumn(
                name: "IsRetired",
                table: "CharacterSheets");

            migrationBuilder.DropColumn(
                name: "PortraitImagePath",
                table: "CharacterSheets");

            migrationBuilder.DropColumn(
                name: "IsArchived",
                table: "CatalogEntries");
        }
    }
}
