using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Lineages =
    [
        Entry("30000000-0000-0000-0000-000000000001", CatalogEntryType.Lineage, "Humano", new { RacialAdjustment = "Nenhum (todos os atributos no teto padrão 5)", RacialTrait = "Adaptável: pode trocar 1 Aptidão escolhida na criação, 1x na campanha" }),
        Entry("30000000-0000-0000-0000-000000000002", CatalogEntryType.Lineage, "Anão", new { RacialAdjustment = "+1 máx. Vigor / −1 máx. Controle", RacialTrait = "Resistência a venenos e doenças" }),
        Entry("30000000-0000-0000-0000-000000000003", CatalogEntryType.Lineage, "Elfo", new { RacialAdjustment = "+1 máx. Percepção / −1 máx. Corpo", RacialTrait = "Visão em baixa luminosidade" }),
        Entry("30000000-0000-0000-0000-000000000004", CatalogEntryType.Lineage, "Meio-Orc", new { RacialAdjustment = "+1 máx. Corpo / −1 máx. Intelecto", RacialTrait = "1x por expedição, ignora uma penalidade de ferimento leve" }),
        Entry("30000000-0000-0000-0000-000000000005", CatalogEntryType.Lineage, "Halfling", new { RacialAdjustment = "+1 máx. Controle / −1 máx. Presença", RacialTrait = "-1 dificuldade em testes de Furtividade" }),
        Entry("30000000-0000-0000-0000-000000000006", CatalogEntryType.Lineage, "Gnomo", new { RacialAdjustment = "+1 máx. Intelecto / −1 máx. Vigor", RacialTrait = "-1 dificuldade no primeiro teste de qualquer perícia de Artesanato aprendida" }),
        Entry("30000000-0000-0000-0000-000000000007", CatalogEntryType.Lineage, "Meio-Elfo", new { RacialAdjustment = "Jogador escolhe livremente qual atributo recebe +1 e qual recebe −1", RacialTrait = "Aptidão extra pode ser trocada 1x (versatilidade)" }),
        Entry("30000000-0000-0000-0000-000000000008", CatalogEntryType.Lineage, "Draconato", new { RacialAdjustment = "+1 máx. Presença / −1 máx. Controle", RacialTrait = "Resistência a um tipo elemental (escolhido na criação)" }),
        Entry("30000000-0000-0000-0000-000000000009", CatalogEntryType.Lineage, "Descendente Sombrio", new { RacialAdjustment = "+1 máx. Vontade / −1 máx. Presença", RacialTrait = "Resistência a medo sobrenatural" }),
        Entry("30000000-0000-0000-0000-000000000010", CatalogEntryType.Lineage, "Fragmentado", new { RacialAdjustment = "+1 máx. Afinidade / −1 máx. Vigor", RacialTrait = "Sente a proximidade de Rupturas e instabilidade dimensional — liga-se diretamente à cosmologia. Rara, exige aprovação do Mestre." }),
    ];
}
