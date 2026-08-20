using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Skills =
    [
        // Combate — Armas (Controle; Corpo em golpes brutos)
        Entry("60000000-0000-0000-0000-000000000001", CatalogEntryType.Skill, "Espadas", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000002", CatalogEntryType.Skill, "Machados", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000003", CatalogEntryType.Skill, "Martelos", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000004", CatalogEntryType.Skill, "Lanças", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000005", CatalogEntryType.Skill, "Armas Improvisadas", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000006", CatalogEntryType.Skill, "Armas Exóticas", new { Area = "Combate — Armas", RelatedAttribute = "Controle" }),

        // Combate — Defesa (Controle/Vigor)
        Entry("60000000-0000-0000-0000-000000000007", CatalogEntryType.Skill, "Escudos", new { Area = "Combate — Defesa", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000008", CatalogEntryType.Skill, "Armaduras", new { Area = "Combate — Defesa", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000009", CatalogEntryType.Skill, "Esquiva", new { Area = "Combate — Defesa", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000010", CatalogEntryType.Skill, "Bloqueio", new { Area = "Combate — Defesa", RelatedAttribute = "Vigor" }),

        // Combate Corporal (Corpo/Controle)
        Entry("60000000-0000-0000-0000-000000000011", CatalogEntryType.Skill, "Artes Marciais", new { Area = "Combate Corporal", RelatedAttribute = "Corpo" }),
        Entry("60000000-0000-0000-0000-000000000012", CatalogEntryType.Skill, "Luta Desarmada", new { Area = "Combate Corporal", RelatedAttribute = "Corpo" }),
        Entry("60000000-0000-0000-0000-000000000013", CatalogEntryType.Skill, "Agarramento", new { Area = "Combate Corporal", RelatedAttribute = "Corpo" }),

        // Combate à Distância (Controle)
        Entry("60000000-0000-0000-0000-000000000014", CatalogEntryType.Skill, "Arcos", new { Area = "Combate à Distância", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000015", CatalogEntryType.Skill, "Bestas", new { Area = "Combate à Distância", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000016", CatalogEntryType.Skill, "Armas de Arremesso", new { Area = "Combate à Distância", RelatedAttribute = "Controle" }),

        // Exploração (Percepção/Vigor/Controle)
        Entry("60000000-0000-0000-0000-000000000017", CatalogEntryType.Skill, "Percepção", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000018", CatalogEntryType.Skill, "Rastreamento", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000019", CatalogEntryType.Skill, "Sobrevivência", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000020", CatalogEntryType.Skill, "Navegação", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000021", CatalogEntryType.Skill, "Furtividade", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000022", CatalogEntryType.Skill, "Armadilhas", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000023", CatalogEntryType.Skill, "Exploração de Dungeon", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000024", CatalogEntryType.Skill, "Escalada", new { Area = "Exploração", RelatedAttribute = "Percepção" }),
        Entry("60000000-0000-0000-0000-000000000025", CatalogEntryType.Skill, "Natação", new { Area = "Exploração", RelatedAttribute = "Percepção" }),

        // Conhecimento (Intelecto)
        Entry("60000000-0000-0000-0000-000000000026", CatalogEntryType.Skill, "História", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000027", CatalogEntryType.Skill, "Geografia", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000028", CatalogEntryType.Skill, "Criaturas", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000029", CatalogEntryType.Skill, "Religião", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000030", CatalogEntryType.Skill, "Linguagens", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000031", CatalogEntryType.Skill, "Estratégia", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000032", CatalogEntryType.Skill, "Dungeonologia", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000033", CatalogEntryType.Skill, "Conhecimento de Animais", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000034", CatalogEntryType.Skill, "Ocultismo", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000035", CatalogEntryType.Skill, "Avaliação", new { Area = "Conhecimento", RelatedAttribute = "Intelecto" }),

        // Cura (Intelecto/Percepção)
        Entry("60000000-0000-0000-0000-000000000036", CatalogEntryType.Skill, "Medicina", new { Area = "Cura", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000037", CatalogEntryType.Skill, "Cirurgia", new { Area = "Cura", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000038", CatalogEntryType.Skill, "Farmacologia", new { Area = "Cura", RelatedAttribute = "Intelecto" }),

        // Artesanato (Controle/Intelecto)
        Entry("60000000-0000-0000-0000-000000000039", CatalogEntryType.Skill, "Ferraria", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000040", CatalogEntryType.Skill, "Carpintaria", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000041", CatalogEntryType.Skill, "Alfaiataria", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000042", CatalogEntryType.Skill, "Engenharia", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000043", CatalogEntryType.Skill, "Construção", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000044", CatalogEntryType.Skill, "Criação de Equipamentos", new { Area = "Artesanato", RelatedAttribute = "Controle" }),
        Entry("60000000-0000-0000-0000-000000000045", CatalogEntryType.Skill, "Culinária", new { Area = "Artesanato", RelatedAttribute = "Controle" }),

        // Alquimia (Intelecto)
        Entry("60000000-0000-0000-0000-000000000046", CatalogEntryType.Skill, "Poções", new { Area = "Alquimia", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000047", CatalogEntryType.Skill, "Venenos", new { Area = "Alquimia", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000048", CatalogEntryType.Skill, "Materiais", new { Area = "Alquimia", RelatedAttribute = "Intelecto" }),
        Entry("60000000-0000-0000-0000-000000000049", CatalogEntryType.Skill, "Transmutação", new { Area = "Alquimia", RelatedAttribute = "Intelecto" }),

        // Magia (Afinidade)
        Entry("60000000-0000-0000-0000-000000000050", CatalogEntryType.Skill, "Controle Mágico", new { Area = "Magia", RelatedAttribute = "Afinidade" }),
        Entry("60000000-0000-0000-0000-000000000051", CatalogEntryType.Skill, "Teoria Arcana", new { Area = "Magia", RelatedAttribute = "Afinidade" }),
        Entry("60000000-0000-0000-0000-000000000052", CatalogEntryType.Skill, "Rituais", new { Area = "Magia", RelatedAttribute = "Afinidade" }),
        Entry("60000000-0000-0000-0000-000000000053", CatalogEntryType.Skill, "Afinidade Elemental", new { Area = "Magia", RelatedAttribute = "Afinidade" }),
        Entry("60000000-0000-0000-0000-000000000054", CatalogEntryType.Skill, "Encantamentos", new { Area = "Magia", RelatedAttribute = "Afinidade" }),

        // Social (Presença/Intelecto)
        Entry("60000000-0000-0000-0000-000000000055", CatalogEntryType.Skill, "Diplomacia", new { Area = "Social", RelatedAttribute = "Presença" }),
        Entry("60000000-0000-0000-0000-000000000056", CatalogEntryType.Skill, "Liderança", new { Area = "Social", RelatedAttribute = "Presença" }),
        Entry("60000000-0000-0000-0000-000000000057", CatalogEntryType.Skill, "Comércio", new { Area = "Social", RelatedAttribute = "Presença" }),
        Entry("60000000-0000-0000-0000-000000000058", CatalogEntryType.Skill, "Intimidação", new { Area = "Social", RelatedAttribute = "Presença" }),
        Entry("60000000-0000-0000-0000-000000000059", CatalogEntryType.Skill, "Manipulação", new { Area = "Social", RelatedAttribute = "Presença" }),
    ];
}
