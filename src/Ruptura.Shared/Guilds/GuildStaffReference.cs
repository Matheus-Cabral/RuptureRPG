namespace Ruptura.Shared.Guilds;

public static class GuildStaffReference
{
    public static readonly IReadOnlyList<string> WorkerTypes =
    [
        GuildStaffTypes.Operario, GuildStaffTypes.Artesao, GuildStaffTypes.Pesquisador,
        GuildStaffTypes.Instrutor, GuildStaffTypes.Mercador, GuildStaffTypes.Medico,
        GuildStaffTypes.Administrador
    ];

    // GDD §10.6.1 mercenary daily salaries by ranking (accented values are valid const/string).
    public static readonly IReadOnlyList<string> MercenaryRankings =
        ["Bronze", "Ferro", "Aço", "Prata", "Ouro", "Mithril", "Adamante", "Lendário"];

    // Default daily salary by type/ranking (Prata/dia). Workers: Operário 3, others skilled 8
    // (GDD fixes only Operário=3, Artesão/Pesquisador=8; the rest default to the skilled rate).
    public static readonly IReadOnlyDictionary<string, int> DefaultSalary = new Dictionary<string, int>
    {
        [GuildStaffTypes.Operario] = 3,
        [GuildStaffTypes.Artesao] = 8,
        [GuildStaffTypes.Pesquisador] = 8,
        [GuildStaffTypes.Instrutor] = 8,
        [GuildStaffTypes.Mercador] = 8,
        [GuildStaffTypes.Medico] = 8,
        [GuildStaffTypes.Administrador] = 8,
        ["Bronze"] = 10, ["Ferro"] = 18, ["Aço"] = 30, ["Prata"] = 50,
        ["Ouro"] = 80, ["Mithril"] = 120, ["Adamante"] = 170, ["Lendário"] = 250,
    };
}
