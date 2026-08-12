using System.Text.Json;
using Ruptura.Domain.Entities;
using Ruptura.Shared.Bestiary;

namespace Ruptura.Infrastructure.Data.Seed;

// The 10 base creatures from GDD §9.5.10, seeded as OFFICIAL examples (GameMasterId = null:
// system-owned, read-only to every GM). Each carries a JSON-serialized CreatureData whose
// authored Attributes/Characteristics/Abilities/Equipment make its DerivedNp (§9.5.5) land
// inside the chosen Category's advisory NP range (§9.5.6). Guids and timestamps are fixed so
// HasData yields a stable migration diff. Category/Behavior tokens use the exact
// BestiaryReference casing; Function prefers the fixed set (§9.5.2).
public static class BestiarySeedData
{
    // Fixed timestamp — using DateTime.UtcNow would make every migration regeneration look
    // like every seed row changed. (Mirrors CatalogSeedData.SeedTimestamp.)
    private static readonly DateTime SeedTimestamp = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static Creature Row(string id, string name, CreatureData data) => new()
    {
        Id = Guid.Parse(id),
        GameMasterId = null,            // official example
        Name = name,
        DataJson = JsonSerializer.Serialize(data),
        CreatedAt = SeedTimestamp,
        UpdatedAt = SeedTimestamp
    };

    private static CreatureCharacteristic Char(string name, string weight) => new() { Name = name, Weight = weight };
    private static CreatureAbility Ability(string name, string tier) => new() { Name = name, Tier = tier };
    private static CreatureEquipment Equip(string name, string rarity) => new() { Name = name, Rarity = rarity };
    private static CreatureNaturalSkill Skill(string name, int points) => new() { Name = name, Points = points };

    public static readonly Creature[] Creatures =
    [
        // 1. Goblin Saqueador — Fraca (20–40). DerivedNp = 28.
        Row("c0000000-0000-0000-0000-000000000001", "Goblin Saqueador", new CreatureData
        {
            Type = "Humanoide Corrompido",
            Function = "Soldado",
            Behavior = "Instintiva",
            Category = "Fraca",
            Attributes = new CreatureAttributes
            {
                Corpo = 4, Controle = 3, Vigor = 3, Presenca = 1,
                Intelecto = 1, Percepcao = 4, Vontade = 2, Afinidade = 1
            },
            NaturalSkills = [Skill("Sentidos Aprimorados", 75), Skill("Furtividade", 50)],
            Characteristics = [Char("Sentidos Aprimorados", "Media")],
            Abilities = [Ability("Investida", "Comum")],
            Equipment = [Equip("Cutelo Improvisado", "Incomum"), Equip("Escudo de Madeira", "Comum")],
            Pv = 18, DefesaPassiva = 12, Deslocamento = 9,
            AtaquePrincipal = "2d10+3 vs Defesa", Dano = "1d8+2",
            Fraqueza = "Foge quando abaixo de 50% dos PV.",
            Recompensas = ["Materiais: sucata e couro", "Conhecimento: rotas de saque goblin"]
        }),

        // 2. Rato Pragado — Fraca (20–40). DerivedNp = 29.
        Row("c0000000-0000-0000-0000-000000000002", "Rato Pragado", new CreatureData
        {
            Type = "Besta",
            Function = "Parasita",
            Behavior = "Instintiva",
            Category = "Fraca",
            Attributes = new CreatureAttributes
            {
                Corpo = 2, Controle = 2, Vigor = 4, Presenca = 1,
                Intelecto = 1, Percepcao = 5, Vontade = 1, Afinidade = 1
            },
            NaturalSkills = [Skill("Olfato Apurado", 75), Skill("Escavar", 50)],
            Characteristics = [Char("Olfato Apurado", "Media"), Char("Doença", "Menor"), Char("Reprodução Rápida", "Menor")],
            Abilities = [Ability("Mordida Infecciosa", "Comum"), Ability("Enxame", "Comum")],
            Equipment = [],
            Pv = 12, DefesaPassiva = 11, Deslocamento = 12,
            AtaquePrincipal = "2d10+2 vs Defesa", Dano = "1d6+1 + Doença",
            Fraqueza = "Vulnerável a fogo.",
            Recompensas = ["Materiais: glândulas infectadas (alquimia)"]
        }),

        // 3. Esqueleto Guardião — Comum (40–70). DerivedNp = 54.
        Row("c0000000-0000-0000-0000-000000000003", "Esqueleto Guardião", new CreatureData
        {
            Type = "Morto-vivo",
            Function = "Guardião",
            Behavior = "Instintiva",
            Category = "Comum",
            Attributes = new CreatureAttributes
            {
                Corpo = 5, Controle = 3, Vigor = 5, Presenca = 2,
                Intelecto = 1, Percepcao = 2, Vontade = 3, Afinidade = 1
            },
            NaturalSkills = [Skill("Vigília", 50), Skill("Bloqueio", 75)],
            Characteristics = [Char("Carapaça Óssea", "Maior"), Char("Imune a Medo", "Media")],
            Abilities = [Ability("Postura Defensiva", "Comum"), Ability("Golpe de Escudo", "Comum")],
            Equipment =
            [
                Equip("Espada Longa Antiga", "Incomum"),
                Equip("Escudo Torre", "Raro"),
                Equip("Armadura de Placas Corroída", "Raro")
            ],
            Pv = 45, DefesaPassiva = 16, Deslocamento = 6,
            AtaquePrincipal = "2d10+5 vs Defesa", Dano = "1d10+3",
            Fraqueza = "Vulnerável a dano contundente.",
            Recompensas = ["Materiais: ossos encantados", "Conhecimento: selo de guarda antigo"]
        }),

        // 4. Cultista Corrompido — Comum (40–70). DerivedNp = 58.
        Row("c0000000-0000-0000-0000-000000000004", "Cultista Corrompido", new CreatureData
        {
            Type = "Humanoide Corrompido",
            Function = "Soldado",
            Behavior = "Inteligente",
            Category = "Comum",
            Attributes = new CreatureAttributes
            {
                Corpo = 3, Controle = 4, Vigor = 3, Presenca = 3,
                Intelecto = 4, Percepcao = 2, Vontade = 2, Afinidade = 4
            },
            NaturalSkills = [Skill("Ocultismo", 75), Skill("Persuasão", 50)],
            Characteristics = [Char("Marca Corrompida", "Media"), Char("Fanatismo", "Media")],
            Abilities = [Ability("Ritual Menor", "Comum"), Ability("Dardo Sombrio", "Comum")],
            Equipment =
            [
                Equip("Adaga Ritual", "Raro"),
                Equip("Manto Cultista", "Incomum"),
                Equip("Grimório Menor", "Raro"),
                Equip("Amuleto Profano", "Incomum")
            ],
            Pv = 32, DefesaPassiva = 13, Deslocamento = 9,
            AtaquePrincipal = "2d10+4 vs Defesa", Dano = "1d8+2 (sombrio)",
            Fraqueza = "Vontade baixa — fácil de intimidar.",
            Recompensas = ["Materiais: componentes rituais", "Conhecimento: nome do culto"]
        }),

        // 5. Aranha das Profundezas — Veterana (70–105). DerivedNp = 86.
        Row("c0000000-0000-0000-0000-000000000005", "Aranha das Profundezas", new CreatureData
        {
            Type = "Besta",
            Function = "Predador",
            Behavior = "Instintiva",
            Category = "Veterana",
            Attributes = new CreatureAttributes
            {
                Corpo = 6, Controle = 5, Vigor = 4, Presenca = 2,
                Intelecto = 1, Percepcao = 7, Vontade = 2, Afinidade = 1
            },
            NaturalSkills = [Skill("Furtividade", 100), Skill("Escalar", 75), Skill("Emboscar", 75)],
            Characteristics =
            [
                Char("Veneno Potente", "Suprema"),
                Char("Camuflagem Natural", "Maior"),
                Char("Teia", "Media"),
                Char("Múltiplos Olhos", "Media")
            ],
            Abilities =
            [
                Ability("Picada Venenosa", "Avancada"),
                Ability("Emboscada", "Comum"),
                Ability("Teia Prendedora", "Comum"),
                Ability("Salto", "Comum"),
                Ability("Frenesi", "Avancada")
            ],
            Equipment = [],
            Pv = 60, DefesaPassiva = 15, Deslocamento = 12,
            AtaquePrincipal = "2d10+6 vs Defesa", Dano = "2d6+3 + Veneno",
            Fraqueza = "Sensível a luz forte e a vibração.",
            Recompensas = ["Materiais: veneno e seda (alquimia)", "Conhecimento: mapa das galerias"]
        }),

        // 6. Cavaleiro Corrompido — Elite (105–195). DerivedNp = 127.
        Row("c0000000-0000-0000-0000-000000000006", "Cavaleiro Corrompido", new CreatureData
        {
            Type = "Morto-vivo",
            Function = "Guardião",
            Behavior = "Estrategica",
            Category = "Elite",
            Attributes = new CreatureAttributes
            {
                Corpo = 8, Controle = 6, Vigor = 8, Presenca = 5,
                Intelecto = 3, Percepcao = 4, Vontade = 6, Afinidade = 2
            },
            NaturalSkills = [Skill("Combate com Espada", 100), Skill("Táticas", 75), Skill("Resistência", 100)],
            Characteristics = [Char("Carapaça", "Maior"), Char("Regeneração", "Maior"), Char("Aura de Terror", "Media")],
            Abilities =
            [
                Ability("Golpe Devastador", "Avancada"),
                Ability("Investida Montada", "Avancada"),
                Ability("Brado de Guerra", "Comum")
            ],
            Equipment =
            [
                Equip("Espada Amaldiçoada", "Epico"),
                Equip("Armadura de Placas Negra", "Epico"),
                Equip("Escudo Rúnico", "Raro"),
                Equip("Elmo Assombrado", "Raro")
            ],
            Pv = 120, DefesaPassiva = 19, Deslocamento = 9,
            AtaquePrincipal = "2d10+8 vs Defesa", Dano = "2d10+5",
            Fraqueza = "Vulnerável a magia sagrada.",
            Recompensas = ["Materiais: aço amaldiçoado", "Técnicas: forma de combate caída", "Conhecimento: juramento quebrado"]
        }),

        // 7. Bruxa do Pântano — Elite (105–195). DerivedNp = 126.
        Row("c0000000-0000-0000-0000-000000000007", "Bruxa do Pântano", new CreatureData
        {
            Type = "Aberração",
            Function = "Soldado",
            Behavior = "Estrategica",
            Category = "Elite",
            Attributes = new CreatureAttributes
            {
                Corpo = 3, Controle = 8, Vigor = 4, Presenca = 6,
                Intelecto = 7, Percepcao = 5, Vontade = 7, Afinidade = 8
            },
            NaturalSkills = [Skill("Feitiçaria", 100), Skill("Herbalismo", 75), Skill("Ocultismo", 100)],
            Characteristics =
            [
                Char("Controle Avançado", "Maior"),
                Char("Regeneração Pantanosa", "Media"),
                Char("Corrupção", "Media")
            ],
            Abilities =
            [
                Ability("Maldição de Controle", "Avancada"),
                Ability("Névoa Tóxica", "Avancada"),
                Ability("Invocar Espíritos", "Avancada"),
                Ability("Toque Necrótico", "Comum")
            ],
            Equipment =
            [
                Equip("Cajado do Pântano", "Epico"),
                Equip("Grimório das Marés", "Raro"),
                Equip("Talismãs Ósseos", "Raro")
            ],
            Pv = 90, DefesaPassiva = 14, Deslocamento = 9,
            AtaquePrincipal = "2d10+7 vs Defesa (Controle)", Dano = "2d8+4 (veneno/psíquico)",
            Fraqueza = "Fraca em combate corpo a corpo.",
            Recompensas = ["Materiais: reagentes raros do pântano", "Técnicas: ritual de controle", "Conhecimento: segredos do brejo"]
        }),

        // 8. Golem de Pedra Fragmentado — Campeã (195–340). DerivedNp = 215.
        Row("c0000000-0000-0000-0000-000000000008", "Golem de Pedra Fragmentado", new CreatureData
        {
            Type = "Constructo",
            Function = "Guardião",
            Behavior = "Instintiva",
            Category = "Campea",
            Attributes = new CreatureAttributes
            {
                Corpo = 20, Controle = 8, Vigor = 30, Presenca = 10,
                Intelecto = 1, Percepcao = 6, Vontade = 15, Afinidade = 1
            },
            NaturalSkills =
            [
                Skill("Esmagar", 100), Skill("Resistência", 100),
                Skill("Fortificar", 100), Skill("Bloqueio", 100)
            ],
            Characteristics =
            [
                Char("Carapaça Dupla", "Suprema"),
                Char("Corpo de Pedra", "Suprema"),
                Char("Imune a Veneno", "Media"),
                Char("Imune a Medo", "Media"),
                Char("Massa Colossal", "Maior"),
                Char("Núcleo Arcano", "Maior")
            ],
            Abilities =
            [
                Ability("Punho Sísmico", "Suprema"),
                Ability("Terremoto", "Suprema"),
                Ability("Pisão", "Avancada"),
                Ability("Arremesso de Rocha", "Avancada"),
                Ability("Postura Inabalável", "Avancada"),
                Ability("Regeneração de Pedra", "Avancada")
            ],
            Equipment = [],
            Pv = 240, DefesaPassiva = 22, Deslocamento = 6,
            AtaquePrincipal = "2d10+12 vs Defesa", Dano = "3d10+8",
            Fraqueza = "Núcleo exposto — ponto fraco atacável.",
            Recompensas = ["Materiais: fragmentos de pedra rúnica", "Cristais: núcleo arcano danificado"]
        }),

        // 9. Comandante Espectral — Campeã (195–340). DerivedNp = 215.
        Row("c0000000-0000-0000-0000-000000000009", "Comandante Espectral", new CreatureData
        {
            Type = "Espírito",
            Function = "Soldado",
            Behavior = "Estrategica",
            Category = "Campea",
            Attributes = new CreatureAttributes
            {
                Corpo = 8, Controle = 12, Vigor = 10, Presenca = 18,
                Intelecto = 12, Percepcao = 10, Vontade = 16, Afinidade = 12
            },
            NaturalSkills =
            [
                Skill("Comando", 100), Skill("Táticas", 100),
                Skill("Intimidação", 100), Skill("Estratégia", 75)
            ],
            Characteristics =
            [
                Char("Voo", "Maior"),
                Char("Incorpóreo", "Suprema"),
                Char("Aura de Comando", "Maior"),
                Char("Presença Aterrorizante", "Media")
            ],
            Abilities =
            [
                Ability("Comando Supremo", "Suprema"),
                Ability("Convocar Legião", "Suprema"),
                Ability("Toque Espectral", "Avancada"),
                Ability("Brado Dissonante", "Avancada"),
                Ability("Marcha Fantasma", "Comum")
            ],
            Equipment = [Equip("Lâmina Espectral", "Epico"), Equip("Estandarte Caído", "Raro")],
            Pv = 160, DefesaPassiva = 18, Deslocamento = 12,
            AtaquePrincipal = "2d10+10 vs Defesa", Dano = "2d10+6 (espectral)",
            Fraqueza = "Dissipa-se com luz sagrada ou Selo.",
            Recompensas = ["Materiais: essência espectral", "Técnicas: comando de horda", "Conhecimento: história da legião caída"]
        }),

        // 10. Dragão do Eclipse — Chefe de Arco (430–550+). DerivedNp = 445.
        Row("c0000000-0000-0000-0000-000000000010", "Dragão do Eclipse", new CreatureData
        {
            Type = "Dracônico",
            Function = "Chefe",
            Behavior = "Estrategica",
            Category = "ChefeDeArco",
            Attributes = new CreatureAttributes
            {
                Corpo = 30, Controle = 20, Vigor = 40, Presenca = 30,
                Intelecto = 20, Percepcao = 22, Vontade = 28, Afinidade = 25
            },
            NaturalSkills =
            [
                Skill("Voo", 100), Skill("Combate Aéreo", 100),
                Skill("Percepção Aguçada", 100), Skill("Intimidação", 100),
                Skill("Resistência", 100)
            ],
            Characteristics =
            [
                Char("Voo", "Maior"),
                Char("Regeneração", "Suprema"),
                Char("Ataques Múltiplos", "Suprema"),
                Char("Escamas do Eclipse", "Suprema"),
                Char("Aura de Escuridão", "Maior"),
                Char("Sangue Corrosivo", "Maior"),
                Char("Imune a Medo", "Media")
            ],
            Abilities =
            [
                Ability("Sopro do Eclipse", "Suprema"),
                Ability("Investida Cataclísmica", "Suprema"),
                Ability("Rugido Aterrorizante", "Suprema"),
                Ability("Garras Múltiplas", "Suprema"),
                Ability("Palavra de Eclipse", "Suprema"),
                Ability("Batida de Asas", "Avancada"),
                Ability("Cauda Esmagadora", "Avancada")
            ],
            Equipment = [Equip("Coração do Eclipse", "Divino")],
            Pv = 520, DefesaPassiva = 26, Deslocamento = 18,
            AtaquePrincipal = "2d10+18 vs Defesa", Dano = "4d10+12 (sopro supremo)",
            Fraqueza = "Núcleo exposto após certa fase da luta.",
            Recompensas = ["Materiais: escamas e coração do eclipse", "Técnicas: sopro supremo", "Cristais: fragmento de arco lendário"]
        }),
    ];
}
