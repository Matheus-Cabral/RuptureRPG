using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Techniques =
    [
        // Espadas
        Entry("80000000-0000-0000-0000-000000000001", CatalogEntryType.Technique, "Postura Ofensiva", new { Style = "Espadas", Category = "Postura", PaCost = 1, Effect = "1 PA, +1 dano / −1 Defesa Passiva enquanto mantida" }),
        Entry("80000000-0000-0000-0000-000000000002", CatalogEntryType.Technique, "Golpe Giratório", new { Style = "Espadas", Category = "Técnica", PaCost = 1, Effect = "I (1 PA): atinge 2 alvos em Contato → II (2 PA, Mestre): atinge todos em Contato" }),
        Entry("80000000-0000-0000-0000-000000000003", CatalogEntryType.Technique, "Aparar", new { Style = "Espadas", Category = "Reação", PaCost = 0, Effect = "Reação, +Defesa Passiva contra 1 ataque; se suceder, permite contra-ataque com dano reduzido" }),
        Entry("80000000-0000-0000-0000-000000000004", CatalogEntryType.Technique, "Corte que Divide o Véu", new { Style = "Espadas", Category = "Suprema", PaCost = 3, Effect = "3 PA, 1x/combate: ignora metade da Redução de Dano da armadura e aplica Sangrando" }),

        // Combate Corporal (Luta Desarmada)
        Entry("80000000-0000-0000-0000-000000000005", CatalogEntryType.Technique, "Guarda Fechada", new { Style = "Combate Corporal", Category = "Postura", PaCost = 1, Effect = "1 PA, +2 Defesa Passiva / −1 dano enquanto mantida" }),
        Entry("80000000-0000-0000-0000-000000000006", CatalogEntryType.Technique, "Golpe Articulado", new { Style = "Combate Corporal", Category = "Técnica", PaCost = 1, Effect = "I (1 PA): ataque com chance de Atordoado leve → II (2 PA, Mestre): chance/efeito maior" }),
        Entry("80000000-0000-0000-0000-000000000007", CatalogEntryType.Technique, "Contragolpe", new { Style = "Combate Corporal", Category = "Reação", PaCost = 0, Effect = "Reação, se a Defesa Ativa suceder, aplica dano imediato ao atacante" }),
        Entry("80000000-0000-0000-0000-000000000008", CatalogEntryType.Technique, "Ruptura de Pontos Vitais", new { Style = "Combate Corporal", Category = "Suprema", PaCost = 3, Effect = "3 PA, 1x/combate: ignora totalmente a Redução de Dano da armadura, aplica Ferido Grave" }),

        // Arcos (Distância)
        Entry("80000000-0000-0000-0000-000000000009", CatalogEntryType.Technique, "Mira Calculada", new { Style = "Arcos", Category = "Postura", PaCost = 1, Effect = "1 PA, +1 precisão contra um alvo marcado, mantida até trocar de alvo" }),
        Entry("80000000-0000-0000-0000-000000000010", CatalogEntryType.Technique, "Tiro Encadeado", new { Style = "Arcos", Category = "Técnica", PaCost = 2, Effect = "I (2 PA): atinge 2 alvos na mesma linha → II (3 PA, Mestre): atinge até 4 alvos" }),
        Entry("80000000-0000-0000-0000-000000000011", CatalogEntryType.Technique, "Disparo de Interceptação", new { Style = "Arcos", Category = "Reação", PaCost = 0, Effect = "Reação, ataca um inimigo que entra na Zona Curta" }),
        Entry("80000000-0000-0000-0000-000000000012", CatalogEntryType.Technique, "Flecha que Perfura o Véu", new { Style = "Arcos", Category = "Suprema", PaCost = 3, Effect = "3 PA, 1x/combate: ignora Cobertura (Parcial/Total) e a Redução de Dano da armadura" }),
    ];
}
