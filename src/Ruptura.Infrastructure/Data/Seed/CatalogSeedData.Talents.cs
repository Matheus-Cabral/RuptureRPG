using Ruptura.Domain.Entities;
using Ruptura.Domain.Enums;

namespace Ruptura.Infrastructure.Data.Seed;

public static partial class CatalogSeedData
{
    public static readonly IReadOnlyList<CatalogEntry> Talents =
    [
        Entry("50000000-0000-0000-0000-000000000001", CatalogEntryType.Talent, "Golpe Certeiro", new { Category = "Combate", Effect = "1x por combate, repete um dado de ataque que considere ruim", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000002", CatalogEntryType.Talent, "Reflexos de Combate", new { Category = "Combate", Effect = "+1 na primeira Esquiva de cada combate", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000003", CatalogEntryType.Talent, "Fúria Contida", new { Category = "Combate", Effect = "1x por combate, ignora a primeira penalidade de ferimento leve", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000004", CatalogEntryType.Talent, "Faro para o Perigo", new { Category = "Exploração", Effect = "-1 dificuldade no primeiro teste de Percepção de cada andar", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000005", CatalogEntryType.Talent, "Pé Leve", new { Category = "Exploração", Effect = "Não sofre penalidade de terreno difícil ao se mover sozinho", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000006", CatalogEntryType.Talent, "Instinto de Sobrevivência", new { Category = "Exploração", Effect = "1x por expedição, evita ficar sem uma ração/tocha por um dia", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000007", CatalogEntryType.Talent, "Mãos Habilidosas", new { Category = "Produção", Effect = "Reduz em 1 dia o tempo do primeiro projeto de fabricação de cada interlúdio", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000008", CatalogEntryType.Talent, "Olho Clínico", new { Category = "Produção", Effect = "Identifica automaticamente a Qualidade de um item ao examiná-lo", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000009", CatalogEntryType.Talent, "Precisão Artesanal", new { Category = "Produção", Effect = "1x por interlúdio, trata um resultado \"Sucesso\" de fabricação como \"Grande Sucesso\"", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000010", CatalogEntryType.Talent, "Reciclador", new { Category = "Produção", Effect = "Recupera metade dos materiais ao falhar em uma fabricação", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000011", CatalogEntryType.Talent, "Vislumbre Arcano", new { Category = "Arcanos", Effect = "Sente a presença de magia ativa num raio curto, sem gastar ação", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000012", CatalogEntryType.Talent, "Fôlego Ritual", new { Category = "Arcanos", Effect = "+1 PA disponível especificamente para conjurar magia, 1x por expedição", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000013", CatalogEntryType.Talent, "Toque Elemental", new { Category = "Arcanos", Effect = "Gera um efeito elemental cosmético/mínimo (luz, calor leve, brisa) sem gastar PA", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000014", CatalogEntryType.Talent, "Memória Arcana", new { Category = "Arcanos", Effect = "1x por pesquisa, reduz o tempo necessário em 1 dia", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000015", CatalogEntryType.Talent, "Presença Firme", new { Category = "Social", Effect = "+1 em testes de Intimidação/Liderança quando em desvantagem numérica", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000016", CatalogEntryType.Talent, "Voz Confiável", new { Category = "Social", Effect = "1x por interlúdio, obtém uma informação de um NPC sem precisar de teste", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000017", CatalogEntryType.Talent, "Diplomata Nato", new { Category = "Social", Effect = "-1 dificuldade no primeiro teste de Diplomacia com uma facção desconhecida", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000018", CatalogEntryType.Talent, "Sorte de Recruta", new { Category = "Extraordinário", Effect = "1x por expedição, transforma uma Falha (não crítica) em Sucesso simples", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000019", CatalogEntryType.Talent, "Marca Estranha", new { Category = "Extraordinário", Effect = "Traço sobrenatural pequeno e inexplicado (definido com o Mestre) — narrativamente rico, mecanicamente neutro até ser investigado em jogo", PowerTier = "menor" }),
        Entry("50000000-0000-0000-0000-000000000020", CatalogEntryType.Talent, "Sina Protegida", new { Category = "Extraordinário", Effect = "1x na campanha inteira, sobrevive a um golpe que o mataria, ficando Incapacitado em vez de morto (efeito consumido após o uso)", PowerTier = "menor" }),
    ];
}
