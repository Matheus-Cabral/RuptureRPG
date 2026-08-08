namespace Ruptura.Shared.Guilds;

public static class ResearchReference
{
    // GDD §11.2 research tiers — base required days by complexity.
    public static readonly IReadOnlyDictionary<string, int> RequiredDays = new Dictionary<string, int>
    {
        ["Menor"] = 5, ["Moderada"] = 10, ["Maior"] = 20, ["Suprema"] = 40,
    };

    // Default CG Pesquisa points by complexity (house default — GDD doesn't fix this; overridable).
    public static readonly IReadOnlyDictionary<string, int> DefaultPoints = new Dictionary<string, int>
    {
        ["Menor"] = 1, ["Moderada"] = 2, ["Maior"] = 3, ["Suprema"] = 5,
    };

    public static readonly IReadOnlyList<string> Complexities = ["Menor", "Moderada", "Maior", "Suprema"];
    public static readonly IReadOnlyList<string> Stages = ["Descobrir", "Pesquisar", "Dominar", "Aplicar"];
    public static readonly IReadOnlyList<string> ResearchTypes =
        ["Arcana", "Biológica", "Tecnológica", "Dimensional", "Histórica", "Militar"];

    public static readonly IReadOnlyList<string> CraftingCategories =
        ["Forja", "Alquimia", "Encantamento", "Engenharia", "Artefatos"];
    public static readonly IReadOnlyList<string> CraftingStatuses = ["EmAndamento", "Concluido", "Cancelado"];
    public static readonly IReadOnlyList<string> Qualities =
        ["Comum", "Superior", "Raro", "Épico", "Lendário", "Divino"];
}
