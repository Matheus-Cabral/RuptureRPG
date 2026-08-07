using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedInstallationsAndDoctrines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CatalogEntries",
                columns: new[] { "Id", "CampaignId", "CreatedAt", "CreatedByGameMasterId", "DataJson", "IsArchived", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("d0000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Funda\\u00E7\\u00E3o\",\"Weight\":1,\"LevelCap\":1,\"Prerequisites\":\"Existe desde o in\\u00EDcio\",\"Unlocks\":\"N\\u00FAcleo da Dungeon; n\\u00E3o se constr\\u00F3i nem melhora\",\"NonConstructible\":true}", false, "Portão", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Funda\\u00E7\\u00E3o\",\"Weight\":1,\"LevelCap\":5,\"Prerequisites\":\"Nenhum\",\"Unlocks\":\"Vagas de personagens/trabalhadores (N\\u00EDvel \\u00D7 2)\",\"NonConstructible\":false}", false, "Dormitório", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Funda\\u00E7\\u00E3o\",\"Weight\":1,\"LevelCap\":5,\"Prerequisites\":\"Nenhum\",\"Unlocks\":\"Armazenamento (N\\u00EDvel \\u00D7 50 unidades)\",\"NonConstructible\":false}", false, "Armazém", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Funda\\u00E7\\u00E3o\",\"Weight\":1,\"LevelCap\":5,\"Prerequisites\":\"Nenhum\",\"Unlocks\":\"Treino de combate; Prova\\u00E7\\u00F5es de Corpo/Controle\",\"NonConstructible\":false}", false, "Campo de Treinamento", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Produ\\u00E7\\u00E3o\",\"Weight\":2,\"LevelCap\":5,\"Prerequisites\":\"Armaz\\u00E9m I\",\"Unlocks\":\"Crafting de armas/armaduras (Comum\\u2192Raro em I-II, \\u00C9pico em III\\u002B)\",\"NonConstructible\":false}", false, "Ferraria", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Produ\\u00E7\\u00E3o\",\"Weight\":2,\"LevelCap\":5,\"Prerequisites\":\"Armaz\\u00E9m I\",\"Unlocks\":\"Crafting geral (Comum/Incomum)\",\"NonConstructible\":false}", false, "Oficina", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Produ\\u00E7\\u00E3o\",\"Weight\":2,\"LevelCap\":7,\"Prerequisites\":\"Dormit\\u00F3rio I\",\"Unlocks\":\"Pesquisa Menor/Moderada; Prova\\u00E7\\u00F5es de Intelecto/Percep\\u00E7\\u00E3o\",\"NonConstructible\":false}", false, "Biblioteca", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000008"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Produ\\u00E7\\u00E3o\",\"Weight\":2,\"LevelCap\":5,\"Prerequisites\":\"Dormit\\u00F3rio I\",\"Unlocks\":\"Cura avan\\u00E7ada, recupera\\u00E7\\u00E3o de PV no Interl\\u00FAdio; Prova\\u00E7\\u00E3o de Vigor\",\"NonConstructible\":false}", false, "Enfermaria", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000009"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Especializa\\u00E7\\u00E3o\",\"Weight\":3,\"LevelCap\":5,\"Prerequisites\":\"Biblioteca II\",\"Unlocks\":\"Pesquisa Arcana Maior; Prova\\u00E7\\u00E3o de Afinidade; Encantamento\",\"NonConstructible\":false}", false, "Laboratório Arcano", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000010"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Especializa\\u00E7\\u00E3o\",\"Weight\":3,\"LevelCap\":5,\"Prerequisites\":\"Campo de Treinamento II \\u002B Enfermaria I\",\"Unlocks\":\"Prova\\u00E7\\u00F5es de Presen\\u00E7a/Vontade; T\\u00E9cnicas Supremas; mercen\\u00E1rios avan\\u00E7ados\",\"NonConstructible\":false}", false, "Academia Militar", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000011"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Especializa\\u00E7\\u00E3o\",\"Weight\":3,\"LevelCap\":4,\"Prerequisites\":\"Oficina II\",\"Unlocks\":\"Alquimia avan\\u00E7ada (Venenos/Transmuta\\u00E7\\u00E3o)\",\"NonConstructible\":false}", false, "Jardim Alquímico", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000012"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Especializa\\u00E7\\u00E3o\",\"Weight\":3,\"LevelCap\":4,\"Prerequisites\":\"Ferraria II\",\"Unlocks\":\"Crafting \\u00C9pico\\u002B; Encantamento de armas\",\"NonConstructible\":false}", false, "Oficina de Runas", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000013"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Institucional\",\"Weight\":5,\"LevelCap\":4,\"Prerequisites\":\"Biblioteca III\",\"Unlocks\":\"Cristais de Mem\\u00F3ria; aumenta Capacidade de Forma\\u00E7\\u00E3o (CF)\",\"NonConstructible\":false}", false, "Memorial", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000014"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Institucional\",\"Weight\":5,\"LevelCap\":4,\"Prerequisites\":\"Armaz\\u00E9m III \\u002B Oficina II\",\"Unlocks\":\"Aumenta Capacidade de Suporte (CS); mais Expedi\\u00E7\\u00F5es Secund\\u00E1rias\",\"NonConstructible\":false}", false, "Centro Logístico", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000015"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Institucional\",\"Weight\":5,\"LevelCap\":4,\"Prerequisites\":\"Academia Militar II\",\"Unlocks\":\"Mercen\\u00E1rios de Ranking mais alto; aumenta limite\",\"NonConstructible\":false}", false, "Quartel dos Mercenários", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000016"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Institucional\",\"Weight\":5,\"LevelCap\":4,\"Prerequisites\":\"Laborat\\u00F3rio Arcano III\",\"Unlocks\":\"Pesquisa Suprema; Rituais avan\\u00E7ados; Grim\\u00F3rios raros\",\"NonConstructible\":false}", false, "Torre dos Magos", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000017"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Monumental\",\"Weight\":8,\"LevelCap\":2,\"Prerequisites\":\"Centro Log\\u00EDstico III \\u002B Memorial II\",\"Unlocks\":\"Aumenta Capacidade Institucional (CI); mais Patronos/projetos simult\\u00E2neos\",\"NonConstructible\":false}", false, "Câmara do Conselho", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000018"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Monumental\",\"Weight\":8,\"LevelCap\":2,\"Prerequisites\":\"Memorial III\",\"Unlocks\":\"Guarda Moedas de Pacto com seguran\\u00E7a; habilita Crafting Divino\",\"NonConstructible\":false}", false, "Cofre Divino", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000019"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Monumental\",\"Weight\":8,\"LevelCap\":2,\"Prerequisites\":\"Torre dos Magos III\",\"Unlocks\":\"Prev\\u00EA Rupturas; reduz a Press\\u00E3o base de andares explorados\",\"NonConstructible\":false}", false, "Observatório Dimensional", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d0000000-0000-0000-0000-000000000020"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Monumental\",\"Weight\":8,\"LevelCap\":2,\"Prerequisites\":\"C\\u00E2mara do Conselho I \\u002B Cofre Divino I\",\"Unlocks\":\"Fortalece o Pacto Divino; resist\\u00EAncia a eventos Divinos negativos\",\"NonConstructible\":false}", false, "Santuário do Patrono", 9, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d1000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Bonus\":\"\\u002B10% em Ataque/Dano de Mercen\\u00E1rios e NPCs de combate da Guilda; -1 dia no tempo de Prova\\u00E7\\u00F5es de Corpo/Controle/Presen\\u00E7a/Vontade\"}", false, "Militar", 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d1000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Bonus\":\"\\u002B15% de velocidade em projetos de Pesquisa (reduz tempo); -10% de custo em Recursos para Prova\\u00E7\\u00F5es de Intelecto/Percep\\u00E7\\u00E3o\"}", false, "Acadêmica", 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d1000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Bonus\":\"\\u002B10% em toda venda de materiais excedentes; reduz o \\u00CDndice de Pre\\u00E7os de Infla\\u00E7\\u00E3o em 1 est\\u00E1gio para compras da pr\\u00F3pria Guilda\"}", false, "Comercial", 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d1000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Bonus\":\"\\u002B15% de chance de sucesso em Expedi\\u00E7\\u00F5es Secund\\u00E1rias; -10% no consumo de Comida/\\u00C1gua/Tochas do grupo principal\"}", false, "Exploração", 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d1000000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Bonus\":\"-1 PA adicional em conjura\\u00E7\\u00E3o para todos os personagens da Guilda; -25% no tempo de Prova\\u00E7\\u00E3o de Afinidade\"}", false, "Arcana", 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d1000000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Bonus\":\"-15% no Tempo de Constru\\u00E7\\u00E3o/Melhoria de instala\\u00E7\\u00F5es; \\u002B10% de chance de Grande Sucesso em Crafting\"}", false, "Engenharia", 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d1000000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Bonus\":\"\\u002B20% na Capacidade de Suporte (CS); -10% na Manuten\\u00E7\\u00E3o Di\\u00E1ria\"}", false, "Logística", 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("d1000000-0000-0000-0000-000000000008"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Bonus\":\"Fac\\u00E7\\u00F5es rec\\u00E9m-descobertas come\\u00E7am com \\u002B15 de Reputa\\u00E7\\u00E3o; ganhos de Reputa\\u00E7\\u00E3o de peso Moderado contam como Maior (perdas continuam normais)\"}", false, "Diplomática", 10, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d0000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("d1000000-0000-0000-0000-000000000008"));
        }
    }
}
