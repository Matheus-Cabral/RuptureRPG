using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Ruptura.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedBestiary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Creatures",
                columns: new[] { "Id", "CreatedAt", "DataJson", "GameMasterId", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { new Guid("c0000000-0000-0000-0000-000000000001"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Humanoide Corrompido\",\"Function\":\"Soldado\",\"Behavior\":\"Instintiva\",\"Category\":\"Fraca\",\"Attributes\":{\"Corpo\":4,\"Controle\":3,\"Vigor\":3,\"Presenca\":1,\"Intelecto\":1,\"Percepcao\":4,\"Vontade\":2,\"Afinidade\":1},\"NaturalSkills\":[{\"Name\":\"Sentidos Aprimorados\",\"Points\":75},{\"Name\":\"Furtividade\",\"Points\":50}],\"Characteristics\":[{\"Name\":\"Sentidos Aprimorados\",\"Weight\":\"Media\"}],\"Abilities\":[{\"Name\":\"Investida\",\"Tier\":\"Comum\"}],\"Equipment\":[{\"Name\":\"Cutelo Improvisado\",\"Rarity\":\"Incomum\"},{\"Name\":\"Escudo de Madeira\",\"Rarity\":\"Comum\"}],\"Pv\":18,\"DefesaPassiva\":12,\"Deslocamento\":9,\"AtaquePrincipal\":\"2d10\\u002B3 vs Defesa\",\"Dano\":\"1d8\\u002B2\",\"Fraqueza\":\"Foge quando abaixo de 50% dos PV.\",\"Recompensas\":[\"Materiais: sucata e couro\",\"Conhecimento: rotas de saque goblin\"],\"Notes\":\"\"}", null, "Goblin Saqueador", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000002"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Besta\",\"Function\":\"Parasita\",\"Behavior\":\"Instintiva\",\"Category\":\"Fraca\",\"Attributes\":{\"Corpo\":2,\"Controle\":2,\"Vigor\":4,\"Presenca\":1,\"Intelecto\":1,\"Percepcao\":5,\"Vontade\":1,\"Afinidade\":1},\"NaturalSkills\":[{\"Name\":\"Olfato Apurado\",\"Points\":75},{\"Name\":\"Escavar\",\"Points\":50}],\"Characteristics\":[{\"Name\":\"Olfato Apurado\",\"Weight\":\"Media\"},{\"Name\":\"Doen\\u00E7a\",\"Weight\":\"Menor\"},{\"Name\":\"Reprodu\\u00E7\\u00E3o R\\u00E1pida\",\"Weight\":\"Menor\"}],\"Abilities\":[{\"Name\":\"Mordida Infecciosa\",\"Tier\":\"Comum\"},{\"Name\":\"Enxame\",\"Tier\":\"Comum\"}],\"Equipment\":[],\"Pv\":12,\"DefesaPassiva\":11,\"Deslocamento\":12,\"AtaquePrincipal\":\"2d10\\u002B2 vs Defesa\",\"Dano\":\"1d6\\u002B1 \\u002B Doen\\u00E7a\",\"Fraqueza\":\"Vulner\\u00E1vel a fogo.\",\"Recompensas\":[\"Materiais: gl\\u00E2ndulas infectadas (alquimia)\"],\"Notes\":\"\"}", null, "Rato Pragado", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000003"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Morto-vivo\",\"Function\":\"Guardi\\u00E3o\",\"Behavior\":\"Instintiva\",\"Category\":\"Comum\",\"Attributes\":{\"Corpo\":5,\"Controle\":3,\"Vigor\":5,\"Presenca\":2,\"Intelecto\":1,\"Percepcao\":2,\"Vontade\":3,\"Afinidade\":1},\"NaturalSkills\":[{\"Name\":\"Vig\\u00EDlia\",\"Points\":50},{\"Name\":\"Bloqueio\",\"Points\":75}],\"Characteristics\":[{\"Name\":\"Carapa\\u00E7a \\u00D3ssea\",\"Weight\":\"Maior\"},{\"Name\":\"Imune a Medo\",\"Weight\":\"Media\"}],\"Abilities\":[{\"Name\":\"Postura Defensiva\",\"Tier\":\"Comum\"},{\"Name\":\"Golpe de Escudo\",\"Tier\":\"Comum\"}],\"Equipment\":[{\"Name\":\"Espada Longa Antiga\",\"Rarity\":\"Incomum\"},{\"Name\":\"Escudo Torre\",\"Rarity\":\"Raro\"},{\"Name\":\"Armadura de Placas Corro\\u00EDda\",\"Rarity\":\"Raro\"}],\"Pv\":45,\"DefesaPassiva\":16,\"Deslocamento\":6,\"AtaquePrincipal\":\"2d10\\u002B5 vs Defesa\",\"Dano\":\"1d10\\u002B3\",\"Fraqueza\":\"Vulner\\u00E1vel a dano contundente.\",\"Recompensas\":[\"Materiais: ossos encantados\",\"Conhecimento: selo de guarda antigo\"],\"Notes\":\"\"}", null, "Esqueleto Guardião", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000004"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Humanoide Corrompido\",\"Function\":\"Soldado\",\"Behavior\":\"Inteligente\",\"Category\":\"Comum\",\"Attributes\":{\"Corpo\":3,\"Controle\":4,\"Vigor\":3,\"Presenca\":3,\"Intelecto\":4,\"Percepcao\":2,\"Vontade\":2,\"Afinidade\":4},\"NaturalSkills\":[{\"Name\":\"Ocultismo\",\"Points\":75},{\"Name\":\"Persuas\\u00E3o\",\"Points\":50}],\"Characteristics\":[{\"Name\":\"Marca Corrompida\",\"Weight\":\"Media\"},{\"Name\":\"Fanatismo\",\"Weight\":\"Media\"}],\"Abilities\":[{\"Name\":\"Ritual Menor\",\"Tier\":\"Comum\"},{\"Name\":\"Dardo Sombrio\",\"Tier\":\"Comum\"}],\"Equipment\":[{\"Name\":\"Adaga Ritual\",\"Rarity\":\"Raro\"},{\"Name\":\"Manto Cultista\",\"Rarity\":\"Incomum\"},{\"Name\":\"Grim\\u00F3rio Menor\",\"Rarity\":\"Raro\"},{\"Name\":\"Amuleto Profano\",\"Rarity\":\"Incomum\"}],\"Pv\":32,\"DefesaPassiva\":13,\"Deslocamento\":9,\"AtaquePrincipal\":\"2d10\\u002B4 vs Defesa\",\"Dano\":\"1d8\\u002B2 (sombrio)\",\"Fraqueza\":\"Vontade baixa \\u2014 f\\u00E1cil de intimidar.\",\"Recompensas\":[\"Materiais: componentes rituais\",\"Conhecimento: nome do culto\"],\"Notes\":\"\"}", null, "Cultista Corrompido", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000005"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Besta\",\"Function\":\"Predador\",\"Behavior\":\"Instintiva\",\"Category\":\"Veterana\",\"Attributes\":{\"Corpo\":6,\"Controle\":5,\"Vigor\":4,\"Presenca\":2,\"Intelecto\":1,\"Percepcao\":7,\"Vontade\":2,\"Afinidade\":1},\"NaturalSkills\":[{\"Name\":\"Furtividade\",\"Points\":100},{\"Name\":\"Escalar\",\"Points\":75},{\"Name\":\"Emboscar\",\"Points\":75}],\"Characteristics\":[{\"Name\":\"Veneno Potente\",\"Weight\":\"Suprema\"},{\"Name\":\"Camuflagem Natural\",\"Weight\":\"Maior\"},{\"Name\":\"Teia\",\"Weight\":\"Media\"},{\"Name\":\"M\\u00FAltiplos Olhos\",\"Weight\":\"Media\"}],\"Abilities\":[{\"Name\":\"Picada Venenosa\",\"Tier\":\"Avancada\"},{\"Name\":\"Emboscada\",\"Tier\":\"Comum\"},{\"Name\":\"Teia Prendedora\",\"Tier\":\"Comum\"},{\"Name\":\"Salto\",\"Tier\":\"Comum\"},{\"Name\":\"Frenesi\",\"Tier\":\"Avancada\"}],\"Equipment\":[],\"Pv\":60,\"DefesaPassiva\":15,\"Deslocamento\":12,\"AtaquePrincipal\":\"2d10\\u002B6 vs Defesa\",\"Dano\":\"2d6\\u002B3 \\u002B Veneno\",\"Fraqueza\":\"Sens\\u00EDvel a luz forte e a vibra\\u00E7\\u00E3o.\",\"Recompensas\":[\"Materiais: veneno e seda (alquimia)\",\"Conhecimento: mapa das galerias\"],\"Notes\":\"\"}", null, "Aranha das Profundezas", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000006"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Morto-vivo\",\"Function\":\"Guardi\\u00E3o\",\"Behavior\":\"Estrategica\",\"Category\":\"Elite\",\"Attributes\":{\"Corpo\":8,\"Controle\":6,\"Vigor\":8,\"Presenca\":5,\"Intelecto\":3,\"Percepcao\":4,\"Vontade\":6,\"Afinidade\":2},\"NaturalSkills\":[{\"Name\":\"Combate com Espada\",\"Points\":100},{\"Name\":\"T\\u00E1ticas\",\"Points\":75},{\"Name\":\"Resist\\u00EAncia\",\"Points\":100}],\"Characteristics\":[{\"Name\":\"Carapa\\u00E7a\",\"Weight\":\"Maior\"},{\"Name\":\"Regenera\\u00E7\\u00E3o\",\"Weight\":\"Maior\"},{\"Name\":\"Aura de Terror\",\"Weight\":\"Media\"}],\"Abilities\":[{\"Name\":\"Golpe Devastador\",\"Tier\":\"Avancada\"},{\"Name\":\"Investida Montada\",\"Tier\":\"Avancada\"},{\"Name\":\"Brado de Guerra\",\"Tier\":\"Comum\"}],\"Equipment\":[{\"Name\":\"Espada Amaldi\\u00E7oada\",\"Rarity\":\"Epico\"},{\"Name\":\"Armadura de Placas Negra\",\"Rarity\":\"Epico\"},{\"Name\":\"Escudo R\\u00FAnico\",\"Rarity\":\"Raro\"},{\"Name\":\"Elmo Assombrado\",\"Rarity\":\"Raro\"}],\"Pv\":120,\"DefesaPassiva\":19,\"Deslocamento\":9,\"AtaquePrincipal\":\"2d10\\u002B8 vs Defesa\",\"Dano\":\"2d10\\u002B5\",\"Fraqueza\":\"Vulner\\u00E1vel a magia sagrada.\",\"Recompensas\":[\"Materiais: a\\u00E7o amaldi\\u00E7oado\",\"T\\u00E9cnicas: forma de combate ca\\u00EDda\",\"Conhecimento: juramento quebrado\"],\"Notes\":\"\"}", null, "Cavaleiro Corrompido", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000007"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Aberra\\u00E7\\u00E3o\",\"Function\":\"Soldado\",\"Behavior\":\"Estrategica\",\"Category\":\"Elite\",\"Attributes\":{\"Corpo\":3,\"Controle\":8,\"Vigor\":4,\"Presenca\":6,\"Intelecto\":7,\"Percepcao\":5,\"Vontade\":7,\"Afinidade\":8},\"NaturalSkills\":[{\"Name\":\"Feiti\\u00E7aria\",\"Points\":100},{\"Name\":\"Herbalismo\",\"Points\":75},{\"Name\":\"Ocultismo\",\"Points\":100}],\"Characteristics\":[{\"Name\":\"Controle Avan\\u00E7ado\",\"Weight\":\"Maior\"},{\"Name\":\"Regenera\\u00E7\\u00E3o Pantanosa\",\"Weight\":\"Media\"},{\"Name\":\"Corrup\\u00E7\\u00E3o\",\"Weight\":\"Media\"}],\"Abilities\":[{\"Name\":\"Maldi\\u00E7\\u00E3o de Controle\",\"Tier\":\"Avancada\"},{\"Name\":\"N\\u00E9voa T\\u00F3xica\",\"Tier\":\"Avancada\"},{\"Name\":\"Invocar Esp\\u00EDritos\",\"Tier\":\"Avancada\"},{\"Name\":\"Toque Necr\\u00F3tico\",\"Tier\":\"Comum\"}],\"Equipment\":[{\"Name\":\"Cajado do P\\u00E2ntano\",\"Rarity\":\"Epico\"},{\"Name\":\"Grim\\u00F3rio das Mar\\u00E9s\",\"Rarity\":\"Raro\"},{\"Name\":\"Talism\\u00E3s \\u00D3sseos\",\"Rarity\":\"Raro\"}],\"Pv\":90,\"DefesaPassiva\":14,\"Deslocamento\":9,\"AtaquePrincipal\":\"2d10\\u002B7 vs Defesa (Controle)\",\"Dano\":\"2d8\\u002B4 (veneno/ps\\u00EDquico)\",\"Fraqueza\":\"Fraca em combate corpo a corpo.\",\"Recompensas\":[\"Materiais: reagentes raros do p\\u00E2ntano\",\"T\\u00E9cnicas: ritual de controle\",\"Conhecimento: segredos do brejo\"],\"Notes\":\"\"}", null, "Bruxa do Pântano", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000008"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Constructo\",\"Function\":\"Guardi\\u00E3o\",\"Behavior\":\"Instintiva\",\"Category\":\"Campea\",\"Attributes\":{\"Corpo\":20,\"Controle\":8,\"Vigor\":30,\"Presenca\":10,\"Intelecto\":1,\"Percepcao\":6,\"Vontade\":15,\"Afinidade\":1},\"NaturalSkills\":[{\"Name\":\"Esmagar\",\"Points\":100},{\"Name\":\"Resist\\u00EAncia\",\"Points\":100},{\"Name\":\"Fortificar\",\"Points\":100},{\"Name\":\"Bloqueio\",\"Points\":100}],\"Characteristics\":[{\"Name\":\"Carapa\\u00E7a Dupla\",\"Weight\":\"Suprema\"},{\"Name\":\"Corpo de Pedra\",\"Weight\":\"Suprema\"},{\"Name\":\"Imune a Veneno\",\"Weight\":\"Media\"},{\"Name\":\"Imune a Medo\",\"Weight\":\"Media\"},{\"Name\":\"Massa Colossal\",\"Weight\":\"Maior\"},{\"Name\":\"N\\u00FAcleo Arcano\",\"Weight\":\"Maior\"}],\"Abilities\":[{\"Name\":\"Punho S\\u00EDsmico\",\"Tier\":\"Suprema\"},{\"Name\":\"Terremoto\",\"Tier\":\"Suprema\"},{\"Name\":\"Pis\\u00E3o\",\"Tier\":\"Avancada\"},{\"Name\":\"Arremesso de Rocha\",\"Tier\":\"Avancada\"},{\"Name\":\"Postura Inabal\\u00E1vel\",\"Tier\":\"Avancada\"},{\"Name\":\"Regenera\\u00E7\\u00E3o de Pedra\",\"Tier\":\"Avancada\"}],\"Equipment\":[],\"Pv\":240,\"DefesaPassiva\":22,\"Deslocamento\":6,\"AtaquePrincipal\":\"2d10\\u002B12 vs Defesa\",\"Dano\":\"3d10\\u002B8\",\"Fraqueza\":\"N\\u00FAcleo exposto \\u2014 ponto fraco atac\\u00E1vel.\",\"Recompensas\":[\"Materiais: fragmentos de pedra r\\u00FAnica\",\"Cristais: n\\u00FAcleo arcano danificado\"],\"Notes\":\"\"}", null, "Golem de Pedra Fragmentado", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000009"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Esp\\u00EDrito\",\"Function\":\"Soldado\",\"Behavior\":\"Estrategica\",\"Category\":\"Campea\",\"Attributes\":{\"Corpo\":8,\"Controle\":12,\"Vigor\":10,\"Presenca\":18,\"Intelecto\":12,\"Percepcao\":10,\"Vontade\":16,\"Afinidade\":12},\"NaturalSkills\":[{\"Name\":\"Comando\",\"Points\":100},{\"Name\":\"T\\u00E1ticas\",\"Points\":100},{\"Name\":\"Intimida\\u00E7\\u00E3o\",\"Points\":100},{\"Name\":\"Estrat\\u00E9gia\",\"Points\":75}],\"Characteristics\":[{\"Name\":\"Voo\",\"Weight\":\"Maior\"},{\"Name\":\"Incorp\\u00F3reo\",\"Weight\":\"Suprema\"},{\"Name\":\"Aura de Comando\",\"Weight\":\"Maior\"},{\"Name\":\"Presen\\u00E7a Aterrorizante\",\"Weight\":\"Media\"}],\"Abilities\":[{\"Name\":\"Comando Supremo\",\"Tier\":\"Suprema\"},{\"Name\":\"Convocar Legi\\u00E3o\",\"Tier\":\"Suprema\"},{\"Name\":\"Toque Espectral\",\"Tier\":\"Avancada\"},{\"Name\":\"Brado Dissonante\",\"Tier\":\"Avancada\"},{\"Name\":\"Marcha Fantasma\",\"Tier\":\"Comum\"}],\"Equipment\":[{\"Name\":\"L\\u00E2mina Espectral\",\"Rarity\":\"Epico\"},{\"Name\":\"Estandarte Ca\\u00EDdo\",\"Rarity\":\"Raro\"}],\"Pv\":160,\"DefesaPassiva\":18,\"Deslocamento\":12,\"AtaquePrincipal\":\"2d10\\u002B10 vs Defesa\",\"Dano\":\"2d10\\u002B6 (espectral)\",\"Fraqueza\":\"Dissipa-se com luz sagrada ou Selo.\",\"Recompensas\":[\"Materiais: ess\\u00EAncia espectral\",\"T\\u00E9cnicas: comando de horda\",\"Conhecimento: hist\\u00F3ria da legi\\u00E3o ca\\u00EDda\"],\"Notes\":\"\"}", null, "Comandante Espectral", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) },
                    { new Guid("c0000000-0000-0000-0000-000000000010"), new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "{\"Type\":\"Drac\\u00F4nico\",\"Function\":\"Chefe\",\"Behavior\":\"Estrategica\",\"Category\":\"ChefeDeArco\",\"Attributes\":{\"Corpo\":30,\"Controle\":20,\"Vigor\":40,\"Presenca\":30,\"Intelecto\":20,\"Percepcao\":22,\"Vontade\":28,\"Afinidade\":25},\"NaturalSkills\":[{\"Name\":\"Voo\",\"Points\":100},{\"Name\":\"Combate A\\u00E9reo\",\"Points\":100},{\"Name\":\"Percep\\u00E7\\u00E3o Agu\\u00E7ada\",\"Points\":100},{\"Name\":\"Intimida\\u00E7\\u00E3o\",\"Points\":100},{\"Name\":\"Resist\\u00EAncia\",\"Points\":100}],\"Characteristics\":[{\"Name\":\"Voo\",\"Weight\":\"Maior\"},{\"Name\":\"Regenera\\u00E7\\u00E3o\",\"Weight\":\"Suprema\"},{\"Name\":\"Ataques M\\u00FAltiplos\",\"Weight\":\"Suprema\"},{\"Name\":\"Escamas do Eclipse\",\"Weight\":\"Suprema\"},{\"Name\":\"Aura de Escurid\\u00E3o\",\"Weight\":\"Maior\"},{\"Name\":\"Sangue Corrosivo\",\"Weight\":\"Maior\"},{\"Name\":\"Imune a Medo\",\"Weight\":\"Media\"}],\"Abilities\":[{\"Name\":\"Sopro do Eclipse\",\"Tier\":\"Suprema\"},{\"Name\":\"Investida Catacl\\u00EDsmica\",\"Tier\":\"Suprema\"},{\"Name\":\"Rugido Aterrorizante\",\"Tier\":\"Suprema\"},{\"Name\":\"Garras M\\u00FAltiplas\",\"Tier\":\"Suprema\"},{\"Name\":\"Palavra de Eclipse\",\"Tier\":\"Suprema\"},{\"Name\":\"Batida de Asas\",\"Tier\":\"Avancada\"},{\"Name\":\"Cauda Esmagadora\",\"Tier\":\"Avancada\"}],\"Equipment\":[{\"Name\":\"Cora\\u00E7\\u00E3o do Eclipse\",\"Rarity\":\"Divino\"}],\"Pv\":520,\"DefesaPassiva\":26,\"Deslocamento\":18,\"AtaquePrincipal\":\"2d10\\u002B18 vs Defesa\",\"Dano\":\"4d10\\u002B12 (sopro supremo)\",\"Fraqueza\":\"N\\u00FAcleo exposto ap\\u00F3s certa fase da luta.\",\"Recompensas\":[\"Materiais: escamas e cora\\u00E7\\u00E3o do eclipse\",\"T\\u00E9cnicas: sopro supremo\",\"Cristais: fragmento de arco lend\\u00E1rio\"],\"Notes\":\"\"}", null, "Dragão do Eclipse", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000001"));

            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000002"));

            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000003"));

            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000004"));

            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000005"));

            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000006"));

            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000007"));

            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000008"));

            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000009"));

            migrationBuilder.DeleteData(
                table: "Creatures",
                keyColumn: "Id",
                keyValue: new Guid("c0000000-0000-0000-0000-000000000010"));
        }
    }
}
