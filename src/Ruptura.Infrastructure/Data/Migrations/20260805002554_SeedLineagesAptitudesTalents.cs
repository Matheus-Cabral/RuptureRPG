using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedLineagesAptitudesTalents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CatalogEntries",
                columns: new[] { "Id", "CampaignId", "CreatedAt", "CreatedByGameMasterId", "DataJson", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("30000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"Nenhum (todos os atributos no teto padr\\u00E3o 5)\",\"RacialTrait\":\"Adapt\\u00E1vel: pode trocar 1 Aptid\\u00E3o escolhida na cria\\u00E7\\u00E3o, 1x na campanha\"}", "Humano", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("30000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"\\u002B1 m\\u00E1x. Vigor / \\u22121 m\\u00E1x. Controle\",\"RacialTrait\":\"Resist\\u00EAncia a venenos e doen\\u00E7as\"}", "Anão", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("30000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"\\u002B1 m\\u00E1x. Percep\\u00E7\\u00E3o / \\u22121 m\\u00E1x. Corpo\",\"RacialTrait\":\"Vis\\u00E3o em baixa luminosidade\"}", "Elfo", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("30000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"\\u002B1 m\\u00E1x. Corpo / \\u22121 m\\u00E1x. Intelecto\",\"RacialTrait\":\"1x por expedi\\u00E7\\u00E3o, ignora uma penalidade de ferimento leve\"}", "Meio-Orc", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("30000000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"\\u002B1 m\\u00E1x. Controle / \\u22121 m\\u00E1x. Presen\\u00E7a\",\"RacialTrait\":\"-1 dificuldade em testes de Furtividade\"}", "Halfling", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("30000000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"\\u002B1 m\\u00E1x. Intelecto / \\u22121 m\\u00E1x. Vigor\",\"RacialTrait\":\"-1 dificuldade no primeiro teste de qualquer per\\u00EDcia de Artesanato aprendida\"}", "Gnomo", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("30000000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"Jogador escolhe livremente qual atributo recebe \\u002B1 e qual recebe \\u22121\",\"RacialTrait\":\"Aptid\\u00E3o extra pode ser trocada 1x (versatilidade)\"}", "Meio-Elfo", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("30000000-0000-0000-0000-000000000008"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"\\u002B1 m\\u00E1x. Presen\\u00E7a / \\u22121 m\\u00E1x. Controle\",\"RacialTrait\":\"Resist\\u00EAncia a um tipo elemental (escolhido na cria\\u00E7\\u00E3o)\"}", "Draconato", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("30000000-0000-0000-0000-000000000009"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"\\u002B1 m\\u00E1x. Vontade / \\u22121 m\\u00E1x. Presen\\u00E7a\",\"RacialTrait\":\"Resist\\u00EAncia a medo sobrenatural\"}", "Descendente Sombrio", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("30000000-0000-0000-0000-000000000010"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"RacialAdjustment\":\"\\u002B1 m\\u00E1x. Afinidade / \\u22121 m\\u00E1x. Vigor\",\"RacialTrait\":\"Sente a proximidade de Rupturas e instabilidade dimensional \\u2014 liga-se diretamente \\u00E0 cosmologia. Rara, exige aprova\\u00E7\\u00E3o do Mestre.\"}", "Fragmentado", 2, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("40000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"CoveredAreas\":[\"Combate \\u2014 Armas\",\"Combate \\u2014 Defesa\",\"Combate Corporal\",\"Combate \\u00E0 Dist\\u00E2ncia\"]}", "Combate", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("40000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"CoveredAreas\":[\"Explora\\u00E7\\u00E3o\"]}", "Exploração", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("40000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"CoveredAreas\":[\"Conhecimento\",\"Cura\"]}", "Conhecimento", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("40000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"CoveredAreas\":[\"Artesanato\",\"Alquimia\"]}", "Ofício", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("40000000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"CoveredAreas\":[\"Magia\"]}", "Magia", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("40000000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"CoveredAreas\":[\"Social\"]}", "Liderança", 3, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Combate\",\"Effect\":\"1x por combate, repete um dado de ataque que considere ruim\",\"PowerTier\":\"menor\"}", "Golpe Certeiro", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Combate\",\"Effect\":\"\\u002B1 na primeira Esquiva de cada combate\",\"PowerTier\":\"menor\"}", "Reflexos de Combate", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Combate\",\"Effect\":\"1x por combate, ignora a primeira penalidade de ferimento leve\",\"PowerTier\":\"menor\"}", "Fúria Contida", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Explora\\u00E7\\u00E3o\",\"Effect\":\"-1 dificuldade no primeiro teste de Percep\\u00E7\\u00E3o de cada andar\",\"PowerTier\":\"menor\"}", "Faro para o Perigo", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Explora\\u00E7\\u00E3o\",\"Effect\":\"N\\u00E3o sofre penalidade de terreno dif\\u00EDcil ao se mover sozinho\",\"PowerTier\":\"menor\"}", "Pé Leve", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Explora\\u00E7\\u00E3o\",\"Effect\":\"1x por expedi\\u00E7\\u00E3o, evita ficar sem uma ra\\u00E7\\u00E3o/tocha por um dia\",\"PowerTier\":\"menor\"}", "Instinto de Sobrevivência", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Produ\\u00E7\\u00E3o\",\"Effect\":\"Reduz em 1 dia o tempo do primeiro projeto de fabrica\\u00E7\\u00E3o de cada interl\\u00FAdio\",\"PowerTier\":\"menor\"}", "Mãos Habilidosas", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000008"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Produ\\u00E7\\u00E3o\",\"Effect\":\"Identifica automaticamente a Qualidade de um item ao examin\\u00E1-lo\",\"PowerTier\":\"menor\"}", "Olho Clínico", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000009"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Produ\\u00E7\\u00E3o\",\"Effect\":\"1x por interl\\u00FAdio, trata um resultado \\u0022Sucesso\\u0022 de fabrica\\u00E7\\u00E3o como \\u0022Grande Sucesso\\u0022\",\"PowerTier\":\"menor\"}", "Precisão Artesanal", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000010"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Produ\\u00E7\\u00E3o\",\"Effect\":\"Recupera metade dos materiais ao falhar em uma fabrica\\u00E7\\u00E3o\",\"PowerTier\":\"menor\"}", "Reciclador", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000011"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Arcanos\",\"Effect\":\"Sente a presen\\u00E7a de magia ativa num raio curto, sem gastar a\\u00E7\\u00E3o\",\"PowerTier\":\"menor\"}", "Vislumbre Arcano", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000012"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Arcanos\",\"Effect\":\"\\u002B1 PA dispon\\u00EDvel especificamente para conjurar magia, 1x por expedi\\u00E7\\u00E3o\",\"PowerTier\":\"menor\"}", "Fôlego Ritual", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000013"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Arcanos\",\"Effect\":\"Gera um efeito elemental cosm\\u00E9tico/m\\u00EDnimo (luz, calor leve, brisa) sem gastar PA\",\"PowerTier\":\"menor\"}", "Toque Elemental", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000014"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Arcanos\",\"Effect\":\"1x por pesquisa, reduz o tempo necess\\u00E1rio em 1 dia\",\"PowerTier\":\"menor\"}", "Memória Arcana", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000015"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Social\",\"Effect\":\"\\u002B1 em testes de Intimida\\u00E7\\u00E3o/Lideran\\u00E7a quando em desvantagem num\\u00E9rica\",\"PowerTier\":\"menor\"}", "Presença Firme", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000016"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Social\",\"Effect\":\"1x por interl\\u00FAdio, obt\\u00E9m uma informa\\u00E7\\u00E3o de um NPC sem precisar de teste\",\"PowerTier\":\"menor\"}", "Voz Confiável", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000017"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Social\",\"Effect\":\"-1 dificuldade no primeiro teste de Diplomacia com uma fac\\u00E7\\u00E3o desconhecida\",\"PowerTier\":\"menor\"}", "Diplomata Nato", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000018"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Extraordin\\u00E1rio\",\"Effect\":\"1x por expedi\\u00E7\\u00E3o, transforma uma Falha (n\\u00E3o cr\\u00EDtica) em Sucesso simples\",\"PowerTier\":\"menor\"}", "Sorte de Recruta", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000019"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Extraordin\\u00E1rio\",\"Effect\":\"Tra\\u00E7o sobrenatural pequeno e inexplicado (definido com o Mestre) \\u2014 narrativamente rico, mecanicamente neutro at\\u00E9 ser investigado em jogo\",\"PowerTier\":\"menor\"}", "Marca Estranha", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("50000000-0000-0000-0000-000000000020"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Category\":\"Extraordin\\u00E1rio\",\"Effect\":\"1x na campanha inteira, sobrevive a um golpe que o mataria, ficando Incapacitado em vez de morto (efeito consumido ap\\u00F3s o uso)\",\"PowerTier\":\"menor\"}", "Sina Protegida", 4, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("30000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("40000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("50000000-0000-0000-0000-000000000020"));
        }
    }
}
