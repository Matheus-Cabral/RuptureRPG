using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Installations =
    [
        // Fundação (Peso 1)
        Entry("d0000000-0000-0000-0000-000000000001", CatalogEntryType.Installation, "Portão", new { Category = "Fundação", Weight = 1, LevelCap = 1, Prerequisites = "Existe desde o início", Unlocks = "Núcleo da Dungeon; não se constrói nem melhora", NonConstructible = true }),
        Entry("d0000000-0000-0000-0000-000000000002", CatalogEntryType.Installation, "Dormitório", new { Category = "Fundação", Weight = 1, LevelCap = 5, Prerequisites = "Nenhum", Unlocks = "Vagas de personagens/trabalhadores (Nível × 2)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000003", CatalogEntryType.Installation, "Armazém", new { Category = "Fundação", Weight = 1, LevelCap = 5, Prerequisites = "Nenhum", Unlocks = "Armazenamento (Nível × 50 unidades)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000004", CatalogEntryType.Installation, "Campo de Treinamento", new { Category = "Fundação", Weight = 1, LevelCap = 5, Prerequisites = "Nenhum", Unlocks = "Treino de combate; Provações de Corpo/Controle", NonConstructible = false }),
        // Produção (Peso 2)
        Entry("d0000000-0000-0000-0000-000000000005", CatalogEntryType.Installation, "Ferraria", new { Category = "Produção", Weight = 2, LevelCap = 5, Prerequisites = "Armazém I", Unlocks = "Crafting de armas/armaduras (Comum→Raro em I-II, Épico em III+)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000006", CatalogEntryType.Installation, "Oficina", new { Category = "Produção", Weight = 2, LevelCap = 5, Prerequisites = "Armazém I", Unlocks = "Crafting geral (Comum/Incomum)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000007", CatalogEntryType.Installation, "Biblioteca", new { Category = "Produção", Weight = 2, LevelCap = 7, Prerequisites = "Dormitório I", Unlocks = "Pesquisa Menor/Moderada; Provações de Intelecto/Percepção", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000008", CatalogEntryType.Installation, "Enfermaria", new { Category = "Produção", Weight = 2, LevelCap = 5, Prerequisites = "Dormitório I", Unlocks = "Cura avançada, recuperação de PV no Interlúdio; Provação de Vigor", NonConstructible = false }),
        // Especialização (Peso 3)
        Entry("d0000000-0000-0000-0000-000000000009", CatalogEntryType.Installation, "Laboratório Arcano", new { Category = "Especialização", Weight = 3, LevelCap = 5, Prerequisites = "Biblioteca II", Unlocks = "Pesquisa Arcana Maior; Provação de Afinidade; Encantamento", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000010", CatalogEntryType.Installation, "Academia Militar", new { Category = "Especialização", Weight = 3, LevelCap = 5, Prerequisites = "Campo de Treinamento II + Enfermaria I", Unlocks = "Provações de Presença/Vontade; Técnicas Supremas; mercenários avançados", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000011", CatalogEntryType.Installation, "Jardim Alquímico", new { Category = "Especialização", Weight = 3, LevelCap = 4, Prerequisites = "Oficina II", Unlocks = "Alquimia avançada (Venenos/Transmutação)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000012", CatalogEntryType.Installation, "Oficina de Runas", new { Category = "Especialização", Weight = 3, LevelCap = 4, Prerequisites = "Ferraria II", Unlocks = "Crafting Épico+; Encantamento de armas", NonConstructible = false }),
        // Institucional (Peso 5)
        Entry("d0000000-0000-0000-0000-000000000013", CatalogEntryType.Installation, "Memorial", new { Category = "Institucional", Weight = 5, LevelCap = 4, Prerequisites = "Biblioteca III", Unlocks = "Cristais de Memória; aumenta Capacidade de Formação (CF)", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000014", CatalogEntryType.Installation, "Centro Logístico", new { Category = "Institucional", Weight = 5, LevelCap = 4, Prerequisites = "Armazém III + Oficina II", Unlocks = "Aumenta Capacidade de Suporte (CS); mais Expedições Secundárias", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000015", CatalogEntryType.Installation, "Quartel dos Mercenários", new { Category = "Institucional", Weight = 5, LevelCap = 4, Prerequisites = "Academia Militar II", Unlocks = "Mercenários de Ranking mais alto; aumenta limite", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000016", CatalogEntryType.Installation, "Torre dos Magos", new { Category = "Institucional", Weight = 5, LevelCap = 4, Prerequisites = "Laboratório Arcano III", Unlocks = "Pesquisa Suprema; Rituais avançados; Grimórios raros", NonConstructible = false }),
        // Monumental (Peso 8)
        Entry("d0000000-0000-0000-0000-000000000017", CatalogEntryType.Installation, "Câmara do Conselho", new { Category = "Monumental", Weight = 8, LevelCap = 2, Prerequisites = "Centro Logístico III + Memorial II", Unlocks = "Aumenta Capacidade Institucional (CI); mais Patronos/projetos simultâneos", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000018", CatalogEntryType.Installation, "Cofre Divino", new { Category = "Monumental", Weight = 8, LevelCap = 2, Prerequisites = "Memorial III", Unlocks = "Guarda Moedas de Pacto com segurança; habilita Crafting Divino", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000019", CatalogEntryType.Installation, "Observatório Dimensional", new { Category = "Monumental", Weight = 8, LevelCap = 2, Prerequisites = "Torre dos Magos III", Unlocks = "Prevê Rupturas; reduz a Pressão base de andares explorados", NonConstructible = false }),
        Entry("d0000000-0000-0000-0000-000000000020", CatalogEntryType.Installation, "Santuário do Patrono", new { Category = "Monumental", Weight = 8, LevelCap = 2, Prerequisites = "Câmara do Conselho I + Cofre Divino I", Unlocks = "Fortalece o Pacto Divino; resistência a eventos Divinos negativos", NonConstructible = false }),
    ];
}
