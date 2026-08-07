namespace Ruptura.Shared.Guilds;

// Canonical worker TypeOrRanking values (Manual §8.4). Stored verbatim on GuildStaff.TypeOrRanking.
// const string can hold accented values — these ARE the persisted strings, matched by the calculator.
public static class GuildStaffTypes
{
    public const string Operario = "Operário";
    public const string Artesao = "Artesão";
    public const string Pesquisador = "Pesquisador";
    public const string Instrutor = "Instrutor";
    public const string Mercador = "Mercador";
    public const string Medico = "Médico";
    public const string Administrador = "Administrador";
}
