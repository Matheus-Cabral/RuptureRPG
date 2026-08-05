using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Spells =
    [
        // Evocação
        Entry("70000000-0000-0000-0000-000000000001", CatalogEntryType.Spell, "Lança de Fogo", new { School = "Evocação", ComplexityPaCost = 1, Range = "Contato/Curta", Area = "Único Alvo", Duration = "Instantânea", Test = "Oposto vs. Vontade/Afinidade", Effect = "Dano de fogo instantâneo a 1 alvo" }),
        Entry("70000000-0000-0000-0000-000000000002", CatalogEntryType.Spell, "Rajada Flamejante", new { School = "Evocação", ComplexityPaCost = 2, Range = "Média", Area = "Linha", Duration = "Instantânea", Test = "Oposto vs. Vontade/Afinidade", Effect = "Dano maior + ignição leve" }),
        Entry("70000000-0000-0000-0000-000000000003", CatalogEntryType.Spell, "Tempestade de Chamas", new { School = "Evocação", ComplexityPaCost = 3, Range = "Média", Area = "Área Pequena", Duration = "2 turnos", Test = "Oposto vs. Vontade/Afinidade", Effect = "Dano contínuo por 2 turnos" }),

        // Abjuração
        Entry("70000000-0000-0000-0000-000000000004", CatalogEntryType.Spell, "Escudo Arcano", new { School = "Abjuração", ComplexityPaCost = 1, Range = "Pessoal", Area = "Único Alvo", Duration = "1 turno", Test = "Absoluto", Effect = "+2 Defesa Passiva, 1 turno" }),
        Entry("70000000-0000-0000-0000-000000000005", CatalogEntryType.Spell, "Barreira Protetora", new { School = "Abjuração", ComplexityPaCost = 2, Range = "Pessoal", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "+4 Defesa Passiva, Cena, só a si mesmo" }),
        Entry("70000000-0000-0000-0000-000000000006", CatalogEntryType.Spell, "Muralha Absoluta", new { School = "Abjuração", ComplexityPaCost = 3, Range = "Curta", Area = "Área Pequena", Duration = "Cena", Test = "Absoluto", Effect = "+4 Defesa Passiva à área pequena (aliados), Cena" }),

        // Controle
        Entry("70000000-0000-0000-0000-000000000007", CatalogEntryType.Spell, "Amarras de Vontade", new { School = "Controle", ComplexityPaCost = 1, Range = "Curta", Area = "Único Alvo", Duration = "1 turno", Test = "Oposto vs. Vontade", Effect = "Imobiliza 1 alvo, 1 turno" }),
        Entry("70000000-0000-0000-0000-000000000008", CatalogEntryType.Spell, "Grilhões Arcanos", new { School = "Controle", ComplexityPaCost = 2, Range = "Curta", Area = "Único Alvo", Duration = "2 turnos", Test = "Oposto vs. Vontade", Effect = "Imobiliza + Enfraquecido, 2 turnos" }),
        Entry("70000000-0000-0000-0000-000000000009", CatalogEntryType.Spell, "Prisão de Vontade", new { School = "Controle", ComplexityPaCost = 3, Range = "Curta", Area = "Área Pequena", Duration = "Cena", Test = "Oposto vs. Vontade", Effect = "Imobiliza área pequena, Cena" }),

        // Convocação
        Entry("70000000-0000-0000-0000-000000000010", CatalogEntryType.Spell, "Lâmina Espectral", new { School = "Convocação", ComplexityPaCost = 1, Range = "Pessoal", Area = "Único Alvo", Duration = "1 turno", Test = "Absoluto", Effect = "Invoca arma temporária (1 turno)" }),
        Entry("70000000-0000-0000-0000-000000000011", CatalogEntryType.Spell, "Familiar de Batalha", new { School = "Convocação", ComplexityPaCost = 2, Range = "Curta", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Invoca criatura pequena, Cena" }),
        Entry("70000000-0000-0000-0000-000000000012", CatalogEntryType.Spell, "Avatar Convocado", new { School = "Convocação", ComplexityPaCost = 3, Range = "Curta", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Invoca aliado poderoso, Cena, Conjuração Prolongada" }),

        // Transmutação
        Entry("70000000-0000-0000-0000-000000000013", CatalogEntryType.Spell, "Toque Deformante", new { School = "Transmutação", ComplexityPaCost = 1, Range = "Contato", Area = "Único Alvo", Duration = "Instantânea", Test = "Absoluto", Effect = "Altera superfície/objeto pequeno" }),
        Entry("70000000-0000-0000-0000-000000000014", CatalogEntryType.Spell, "Metamorfose Parcial", new { School = "Transmutação", ComplexityPaCost = 2, Range = "Pessoal", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Altera parte do próprio corpo, ganho utilitário, Cena" }),
        Entry("70000000-0000-0000-0000-000000000015", CatalogEntryType.Spell, "Transfiguração Completa", new { School = "Transmutação", ComplexityPaCost = 3, Range = "Pessoal", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Altera a forma por completo, Cena" }),

        // Ilusão
        Entry("70000000-0000-0000-0000-000000000016", CatalogEntryType.Spell, "Névoa Enganosa", new { School = "Ilusão", ComplexityPaCost = 1, Range = "Pessoal", Area = "Único Alvo", Duration = "Cena", Test = "Absoluto", Effect = "Camufla 1 alvo, +Furtividade" }),
        Entry("70000000-0000-0000-0000-000000000017", CatalogEntryType.Spell, "Duplicata Ilusória", new { School = "Ilusão", ComplexityPaCost = 2, Range = "Pessoal", Area = "Único Alvo", Duration = "Instantânea", Test = "Absoluto", Effect = "Imagem falsa, confunde 1 ataque" }),
        Entry("70000000-0000-0000-0000-000000000018", CatalogEntryType.Spell, "Véu da Mentira", new { School = "Ilusão", ComplexityPaCost = 3, Range = "Curta", Area = "Área Grande", Duration = "Cena", Test = "Oposto vs. Vontade", Effect = "Ilude um grupo/área inteira, Cena" }),

        // Necromancia
        Entry("70000000-0000-0000-0000-000000000019", CatalogEntryType.Spell, "Toque Debilitante", new { School = "Necromancia", ComplexityPaCost = 1, Range = "Contato", Area = "Único Alvo", Duration = "Instantânea", Test = "Oposto vs. Vontade", Effect = "Dreno pequeno de PV/Vigor" }),
        Entry("70000000-0000-0000-0000-000000000020", CatalogEntryType.Spell, "Sopro Sombrio", new { School = "Necromancia", ComplexityPaCost = 2, Range = "Curta", Area = "Área Pequena", Duration = "Instantânea", Test = "Oposto vs. Vontade", Effect = "Dreno em área pequena" }),
        Entry("70000000-0000-0000-0000-000000000021", CatalogEntryType.Spell, "Chamado da Sepultura", new { School = "Necromancia", ComplexityPaCost = 3, Range = "Curta", Area = "Único Alvo", Duration = "Conjuração Prolongada", Test = "Absoluto", Effect = "Invoca mortos-vivos menores temporários, Conjuração Prolongada" }),

        // Adivinação
        Entry("70000000-0000-0000-0000-000000000022", CatalogEntryType.Spell, "Vislumbre", new { School = "Adivinação", ComplexityPaCost = 1, Range = "Curta", Area = "Único Alvo", Duration = "Instantânea", Test = "Absoluto", Effect = "Revela 1 informação simples sobre alvo/ambiente" }),
        Entry("70000000-0000-0000-0000-000000000023", CatalogEntryType.Spell, "Leitura do Fio do Destino", new { School = "Adivinação", ComplexityPaCost = 2, Range = "Curta", Area = "Único Alvo", Duration = "Instantânea", Test = "Absoluto", Effect = "Prevê a próxima ação de 1 alvo, concede Vantagem" }),
        Entry("70000000-0000-0000-0000-000000000024", CatalogEntryType.Spell, "Olho Onisciente", new { School = "Adivinação", ComplexityPaCost = 3, Range = "Cena", Area = "Área Grande", Duration = "Cena", Test = "Absoluto", Effect = "Revela mapa/segredos de uma área inteira, Cena" }),
    ];
}
