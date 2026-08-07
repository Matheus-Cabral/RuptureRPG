using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Doctrines =
    [
        Entry("d1000000-0000-0000-0000-000000000001", CatalogEntryType.Doctrine, "Militar", new { Bonus = "+10% em Ataque/Dano de Mercenários e NPCs de combate da Guilda; -1 dia no tempo de Provações de Corpo/Controle/Presença/Vontade" }),
        Entry("d1000000-0000-0000-0000-000000000002", CatalogEntryType.Doctrine, "Acadêmica", new { Bonus = "+15% de velocidade em projetos de Pesquisa (reduz tempo); -10% de custo em Recursos para Provações de Intelecto/Percepção" }),
        Entry("d1000000-0000-0000-0000-000000000003", CatalogEntryType.Doctrine, "Comercial", new { Bonus = "+10% em toda venda de materiais excedentes; reduz o Índice de Preços de Inflação em 1 estágio para compras da própria Guilda" }),
        Entry("d1000000-0000-0000-0000-000000000004", CatalogEntryType.Doctrine, "Exploração", new { Bonus = "+15% de chance de sucesso em Expedições Secundárias; -10% no consumo de Comida/Água/Tochas do grupo principal" }),
        Entry("d1000000-0000-0000-0000-000000000005", CatalogEntryType.Doctrine, "Arcana", new { Bonus = "-1 PA adicional em conjuração para todos os personagens da Guilda; -25% no tempo de Provação de Afinidade" }),
        Entry("d1000000-0000-0000-0000-000000000006", CatalogEntryType.Doctrine, "Engenharia", new { Bonus = "-15% no Tempo de Construção/Melhoria de instalações; +10% de chance de Grande Sucesso em Crafting" }),
        Entry("d1000000-0000-0000-0000-000000000007", CatalogEntryType.Doctrine, "Logística", new { Bonus = "+20% na Capacidade de Suporte (CS); -10% na Manutenção Diária" }),
        Entry("d1000000-0000-0000-0000-000000000008", CatalogEntryType.Doctrine, "Diplomática", new { Bonus = "Facções recém-descobertas começam com +15 de Reputação; ganhos de Reputação de peso Moderado contam como Maior (perdas continuam normais)" }),
    ];
}
