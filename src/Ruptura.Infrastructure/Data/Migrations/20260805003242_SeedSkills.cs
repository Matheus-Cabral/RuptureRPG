using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSkills : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CatalogEntries",
                columns: new[] { "Id", "CampaignId", "CreatedAt", "CreatedByGameMasterId", "DataJson", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("60000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Armas\",\"RelatedAttribute\":\"Controle\"}", "Espadas", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Armas\",\"RelatedAttribute\":\"Controle\"}", "Machados", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Armas\",\"RelatedAttribute\":\"Controle\"}", "Martelos", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Armas\",\"RelatedAttribute\":\"Controle\"}", "Lanças", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Armas\",\"RelatedAttribute\":\"Controle\"}", "Armas Improvisadas", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Armas\",\"RelatedAttribute\":\"Controle\"}", "Armas Exóticas", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Defesa\",\"RelatedAttribute\":\"Controle\"}", "Escudos", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000008"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Defesa\",\"RelatedAttribute\":\"Controle\"}", "Armaduras", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000009"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Defesa\",\"RelatedAttribute\":\"Controle\"}", "Esquiva", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000010"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u2014 Defesa\",\"RelatedAttribute\":\"Controle\"}", "Bloqueio", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000011"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate Corporal\",\"RelatedAttribute\":\"Corpo\"}", "Artes Marciais", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000012"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate Corporal\",\"RelatedAttribute\":\"Corpo\"}", "Luta Desarmada", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000013"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate Corporal\",\"RelatedAttribute\":\"Corpo\"}", "Agarramento", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000014"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u00E0 Dist\\u00E2ncia\",\"RelatedAttribute\":\"Controle\"}", "Arcos", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000015"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u00E0 Dist\\u00E2ncia\",\"RelatedAttribute\":\"Controle\"}", "Bestas", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000016"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Combate \\u00E0 Dist\\u00E2ncia\",\"RelatedAttribute\":\"Controle\"}", "Armas de Arremesso", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000017"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Explora\\u00E7\\u00E3o\",\"RelatedAttribute\":\"Percep\\u00E7\\u00E3o\"}", "Percepção", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000018"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Explora\\u00E7\\u00E3o\",\"RelatedAttribute\":\"Percep\\u00E7\\u00E3o\"}", "Rastreamento", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000019"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Explora\\u00E7\\u00E3o\",\"RelatedAttribute\":\"Percep\\u00E7\\u00E3o\"}", "Sobrevivência", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000020"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Explora\\u00E7\\u00E3o\",\"RelatedAttribute\":\"Percep\\u00E7\\u00E3o\"}", "Navegação", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000021"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Explora\\u00E7\\u00E3o\",\"RelatedAttribute\":\"Percep\\u00E7\\u00E3o\"}", "Furtividade", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000022"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Explora\\u00E7\\u00E3o\",\"RelatedAttribute\":\"Percep\\u00E7\\u00E3o\"}", "Armadilhas", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000023"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Explora\\u00E7\\u00E3o\",\"RelatedAttribute\":\"Percep\\u00E7\\u00E3o\"}", "Exploração de Dungeon", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000024"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Explora\\u00E7\\u00E3o\",\"RelatedAttribute\":\"Percep\\u00E7\\u00E3o\"}", "Escalada", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000025"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Explora\\u00E7\\u00E3o\",\"RelatedAttribute\":\"Percep\\u00E7\\u00E3o\"}", "Natação", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000026"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "História", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000027"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "Geografia", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000028"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "Criaturas", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000029"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "Religião", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000030"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "Linguagens", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000031"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "Estratégia", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000032"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "Dungeonologia", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000033"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "Conhecimento de Animais", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000034"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "Ocultismo", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000035"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Conhecimento\",\"RelatedAttribute\":\"Intelecto\"}", "Avaliação", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000036"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Cura\",\"RelatedAttribute\":\"Intelecto\"}", "Medicina", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000037"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Cura\",\"RelatedAttribute\":\"Intelecto\"}", "Cirurgia", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000038"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Cura\",\"RelatedAttribute\":\"Intelecto\"}", "Farmacologia", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000039"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Artesanato\",\"RelatedAttribute\":\"Controle\"}", "Ferraria", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000040"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Artesanato\",\"RelatedAttribute\":\"Controle\"}", "Carpintaria", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000041"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Artesanato\",\"RelatedAttribute\":\"Controle\"}", "Alfaiataria", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000042"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Artesanato\",\"RelatedAttribute\":\"Controle\"}", "Engenharia", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000043"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Artesanato\",\"RelatedAttribute\":\"Controle\"}", "Construção", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000044"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Artesanato\",\"RelatedAttribute\":\"Controle\"}", "Criação de Equipamentos", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000045"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Artesanato\",\"RelatedAttribute\":\"Controle\"}", "Culinária", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000046"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Alquimia\",\"RelatedAttribute\":\"Intelecto\"}", "Poções", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000047"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Alquimia\",\"RelatedAttribute\":\"Intelecto\"}", "Venenos", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000048"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Alquimia\",\"RelatedAttribute\":\"Intelecto\"}", "Materiais", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000049"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Alquimia\",\"RelatedAttribute\":\"Intelecto\"}", "Transmutação", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000050"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Magia\",\"RelatedAttribute\":\"Afinidade\"}", "Controle Mágico", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000051"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Magia\",\"RelatedAttribute\":\"Afinidade\"}", "Teoria Arcana", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000052"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Magia\",\"RelatedAttribute\":\"Afinidade\"}", "Rituais", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000053"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Magia\",\"RelatedAttribute\":\"Afinidade\"}", "Afinidade Elemental", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000054"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Magia\",\"RelatedAttribute\":\"Afinidade\"}", "Encantamentos", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000055"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Social\",\"RelatedAttribute\":\"Presen\\u00E7a\"}", "Diplomacia", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000056"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Social\",\"RelatedAttribute\":\"Presen\\u00E7a\"}", "Liderança", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000057"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Social\",\"RelatedAttribute\":\"Presen\\u00E7a\"}", "Comércio", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000058"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Social\",\"RelatedAttribute\":\"Presen\\u00E7a\"}", "Intimidação", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("60000000-0000-0000-0000-000000000059"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Area\":\"Social\",\"RelatedAttribute\":\"Presen\\u00E7a\"}", "Manipulação", 5, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000025"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000026"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000027"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000028"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000029"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000030"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000031"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000032"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000033"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000034"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000035"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000036"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000037"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000038"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000039"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000040"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000041"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000042"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000043"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000044"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000045"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000046"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000047"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000048"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000049"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000050"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000051"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000052"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000053"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000054"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000055"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000056"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000057"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000058"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("60000000-0000-0000-0000-000000000059"));
        }
    }
}
