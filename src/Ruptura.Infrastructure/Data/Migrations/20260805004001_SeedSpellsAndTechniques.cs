using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedSpellsAndTechniques : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "CatalogEntries",
                columns: new[] { "Id", "CampaignId", "CreatedAt", "CreatedByGameMasterId", "DataJson", "Name", "Type", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("70000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Evoca\\u00E7\\u00E3o\",\"ComplexityPaCost\":1,\"Range\":\"Contato/Curta\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Instant\\u00E2nea\",\"Test\":\"Oposto vs. Vontade/Afinidade\",\"Effect\":\"Dano de fogo instant\\u00E2neo a 1 alvo\"}", "Lança de Fogo", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Evoca\\u00E7\\u00E3o\",\"ComplexityPaCost\":2,\"Range\":\"M\\u00E9dia\",\"Area\":\"Linha\",\"Duration\":\"Instant\\u00E2nea\",\"Test\":\"Oposto vs. Vontade/Afinidade\",\"Effect\":\"Dano maior \\u002B igni\\u00E7\\u00E3o leve\"}", "Rajada Flamejante", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Evoca\\u00E7\\u00E3o\",\"ComplexityPaCost\":3,\"Range\":\"M\\u00E9dia\",\"Area\":\"\\u00C1rea Pequena\",\"Duration\":\"2 turnos\",\"Test\":\"Oposto vs. Vontade/Afinidade\",\"Effect\":\"Dano cont\\u00EDnuo por 2 turnos\"}", "Tempestade de Chamas", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Abjura\\u00E7\\u00E3o\",\"ComplexityPaCost\":1,\"Range\":\"Pessoal\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"1 turno\",\"Test\":\"Absoluto\",\"Effect\":\"\\u002B2 Defesa Passiva, 1 turno\"}", "Escudo Arcano", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Abjura\\u00E7\\u00E3o\",\"ComplexityPaCost\":2,\"Range\":\"Pessoal\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Cena\",\"Test\":\"Absoluto\",\"Effect\":\"\\u002B4 Defesa Passiva, Cena, s\\u00F3 a si mesmo\"}", "Barreira Protetora", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Abjura\\u00E7\\u00E3o\",\"ComplexityPaCost\":3,\"Range\":\"Curta\",\"Area\":\"\\u00C1rea Pequena\",\"Duration\":\"Cena\",\"Test\":\"Absoluto\",\"Effect\":\"\\u002B4 Defesa Passiva \\u00E0 \\u00E1rea pequena (aliados), Cena\"}", "Muralha Absoluta", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Controle\",\"ComplexityPaCost\":1,\"Range\":\"Curta\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"1 turno\",\"Test\":\"Oposto vs. Vontade\",\"Effect\":\"Imobiliza 1 alvo, 1 turno\"}", "Amarras de Vontade", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000008"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Controle\",\"ComplexityPaCost\":2,\"Range\":\"Curta\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"2 turnos\",\"Test\":\"Oposto vs. Vontade\",\"Effect\":\"Imobiliza \\u002B Enfraquecido, 2 turnos\"}", "Grilhões Arcanos", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000009"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Controle\",\"ComplexityPaCost\":3,\"Range\":\"Curta\",\"Area\":\"\\u00C1rea Pequena\",\"Duration\":\"Cena\",\"Test\":\"Oposto vs. Vontade\",\"Effect\":\"Imobiliza \\u00E1rea pequena, Cena\"}", "Prisão de Vontade", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000010"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Convoca\\u00E7\\u00E3o\",\"ComplexityPaCost\":1,\"Range\":\"Pessoal\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"1 turno\",\"Test\":\"Absoluto\",\"Effect\":\"Invoca arma tempor\\u00E1ria (1 turno)\"}", "Lâmina Espectral", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000011"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Convoca\\u00E7\\u00E3o\",\"ComplexityPaCost\":2,\"Range\":\"Curta\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Cena\",\"Test\":\"Absoluto\",\"Effect\":\"Invoca criatura pequena, Cena\"}", "Familiar de Batalha", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000012"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Convoca\\u00E7\\u00E3o\",\"ComplexityPaCost\":3,\"Range\":\"Curta\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Cena\",\"Test\":\"Absoluto\",\"Effect\":\"Invoca aliado poderoso, Cena, Conjura\\u00E7\\u00E3o Prolongada\"}", "Avatar Convocado", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000013"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Transmuta\\u00E7\\u00E3o\",\"ComplexityPaCost\":1,\"Range\":\"Contato\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Instant\\u00E2nea\",\"Test\":\"Absoluto\",\"Effect\":\"Altera superf\\u00EDcie/objeto pequeno\"}", "Toque Deformante", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000014"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Transmuta\\u00E7\\u00E3o\",\"ComplexityPaCost\":2,\"Range\":\"Pessoal\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Cena\",\"Test\":\"Absoluto\",\"Effect\":\"Altera parte do pr\\u00F3prio corpo, ganho utilit\\u00E1rio, Cena\"}", "Metamorfose Parcial", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000015"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Transmuta\\u00E7\\u00E3o\",\"ComplexityPaCost\":3,\"Range\":\"Pessoal\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Cena\",\"Test\":\"Absoluto\",\"Effect\":\"Altera a forma por completo, Cena\"}", "Transfiguração Completa", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000016"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Ilus\\u00E3o\",\"ComplexityPaCost\":1,\"Range\":\"Pessoal\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Cena\",\"Test\":\"Absoluto\",\"Effect\":\"Camufla 1 alvo, \\u002BFurtividade\"}", "Névoa Enganosa", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000017"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Ilus\\u00E3o\",\"ComplexityPaCost\":2,\"Range\":\"Pessoal\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Instant\\u00E2nea\",\"Test\":\"Absoluto\",\"Effect\":\"Imagem falsa, confunde 1 ataque\"}", "Duplicata Ilusória", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000018"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Ilus\\u00E3o\",\"ComplexityPaCost\":3,\"Range\":\"Curta\",\"Area\":\"\\u00C1rea Grande\",\"Duration\":\"Cena\",\"Test\":\"Oposto vs. Vontade\",\"Effect\":\"Ilude um grupo/\\u00E1rea inteira, Cena\"}", "Véu da Mentira", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000019"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Necromancia\",\"ComplexityPaCost\":1,\"Range\":\"Contato\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Instant\\u00E2nea\",\"Test\":\"Oposto vs. Vontade\",\"Effect\":\"Dreno pequeno de PV/Vigor\"}", "Toque Debilitante", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000020"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Necromancia\",\"ComplexityPaCost\":2,\"Range\":\"Curta\",\"Area\":\"\\u00C1rea Pequena\",\"Duration\":\"Instant\\u00E2nea\",\"Test\":\"Oposto vs. Vontade\",\"Effect\":\"Dreno em \\u00E1rea pequena\"}", "Sopro Sombrio", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000021"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Necromancia\",\"ComplexityPaCost\":3,\"Range\":\"Curta\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Conjura\\u00E7\\u00E3o Prolongada\",\"Test\":\"Absoluto\",\"Effect\":\"Invoca mortos-vivos menores tempor\\u00E1rios, Conjura\\u00E7\\u00E3o Prolongada\"}", "Chamado da Sepultura", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000022"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Adivina\\u00E7\\u00E3o\",\"ComplexityPaCost\":1,\"Range\":\"Curta\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Instant\\u00E2nea\",\"Test\":\"Absoluto\",\"Effect\":\"Revela 1 informa\\u00E7\\u00E3o simples sobre alvo/ambiente\"}", "Vislumbre", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000023"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Adivina\\u00E7\\u00E3o\",\"ComplexityPaCost\":2,\"Range\":\"Curta\",\"Area\":\"\\u00DAnico Alvo\",\"Duration\":\"Instant\\u00E2nea\",\"Test\":\"Absoluto\",\"Effect\":\"Prev\\u00EA a pr\\u00F3xima a\\u00E7\\u00E3o de 1 alvo, concede Vantagem\"}", "Leitura do Fio do Destino", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("70000000-0000-0000-0000-000000000024"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"School\":\"Adivina\\u00E7\\u00E3o\",\"ComplexityPaCost\":3,\"Range\":\"Cena\",\"Area\":\"\\u00C1rea Grande\",\"Duration\":\"Cena\",\"Test\":\"Absoluto\",\"Effect\":\"Revela mapa/segredos de uma \\u00E1rea inteira, Cena\"}", "Olho Onisciente", 6, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000001"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Espadas\",\"Category\":\"Postura\",\"PaCost\":1,\"Effect\":\"1 PA, \\u002B1 dano / \\u22121 Defesa Passiva enquanto mantida\"}", "Postura Ofensiva", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000002"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Espadas\",\"Category\":\"T\\u00E9cnica\",\"PaCost\":1,\"Effect\":\"I (1 PA): atinge 2 alvos em Contato \\u2192 II (2 PA, Mestre): atinge todos em Contato\"}", "Golpe Giratório", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000003"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Espadas\",\"Category\":\"Rea\\u00E7\\u00E3o\",\"PaCost\":0,\"Effect\":\"Rea\\u00E7\\u00E3o, \\u002BDefesa Passiva contra 1 ataque; se suceder, permite contra-ataque com dano reduzido\"}", "Aparar", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000004"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Espadas\",\"Category\":\"Suprema\",\"PaCost\":3,\"Effect\":\"3 PA, 1x/combate: ignora metade da Redu\\u00E7\\u00E3o de Dano da armadura e aplica Sangrando\"}", "Corte que Divide o Véu", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000005"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Combate Corporal\",\"Category\":\"Postura\",\"PaCost\":1,\"Effect\":\"1 PA, \\u002B2 Defesa Passiva / \\u22121 dano enquanto mantida\"}", "Guarda Fechada", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000006"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Combate Corporal\",\"Category\":\"T\\u00E9cnica\",\"PaCost\":1,\"Effect\":\"I (1 PA): ataque com chance de Atordoado leve \\u2192 II (2 PA, Mestre): chance/efeito maior\"}", "Golpe Articulado", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000007"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Combate Corporal\",\"Category\":\"Rea\\u00E7\\u00E3o\",\"PaCost\":0,\"Effect\":\"Rea\\u00E7\\u00E3o, se a Defesa Ativa suceder, aplica dano imediato ao atacante\"}", "Contragolpe", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000008"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Combate Corporal\",\"Category\":\"Suprema\",\"PaCost\":3,\"Effect\":\"3 PA, 1x/combate: ignora totalmente a Redu\\u00E7\\u00E3o de Dano da armadura, aplica Ferido Grave\"}", "Ruptura de Pontos Vitais", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000009"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Arcos\",\"Category\":\"Postura\",\"PaCost\":1,\"Effect\":\"1 PA, \\u002B1 precis\\u00E3o contra um alvo marcado, mantida at\\u00E9 trocar de alvo\"}", "Mira Calculada", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000010"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Arcos\",\"Category\":\"T\\u00E9cnica\",\"PaCost\":2,\"Effect\":\"I (2 PA): atinge 2 alvos na mesma linha \\u2192 II (3 PA, Mestre): atinge at\\u00E9 4 alvos\"}", "Tiro Encadeado", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000011"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Arcos\",\"Category\":\"Rea\\u00E7\\u00E3o\",\"PaCost\":0,\"Effect\":\"Rea\\u00E7\\u00E3o, ataca um inimigo que entra na Zona Curta\"}", "Disparo de Interceptação", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("80000000-0000-0000-0000-000000000012"), null, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "{\"Style\":\"Arcos\",\"Category\":\"Suprema\",\"PaCost\":3,\"Effect\":\"3 PA, 1x/combate: ignora Cobertura (Parcial/Total) e a Redu\\u00E7\\u00E3o de Dano da armadura\"}", "Flecha que Perfura o Véu", 7, new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000012"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000013"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000014"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000015"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000016"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000017"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000018"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000019"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000020"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000021"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000022"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000023"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("70000000-0000-0000-0000-000000000024"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000010"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000011"));

            migrationBuilder.DeleteData(
                table: "CatalogEntries",
                keyColumn: "Id",
                keyValue: new Guid("80000000-0000-0000-0000-000000000012"));
        }
    }
}
