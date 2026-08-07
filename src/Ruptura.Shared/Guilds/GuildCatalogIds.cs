namespace Ruptura.Shared.Guilds;

// Seeded catalog GUIDs (sub-plan #1). C# has no `const Guid`, so these are static readonly.
// The calculator identifies formula-relevant installations/doctrines by these, never by name.
public static class GuildCatalogIds
{
    // Installations (d0000000-…) — formula-relevant subset + Portão.
    public static readonly Guid Portao = Guid.Parse("d0000000-0000-0000-0000-000000000001");
    public static readonly Guid Dormitorio = Guid.Parse("d0000000-0000-0000-0000-000000000002");
    public static readonly Guid Armazem = Guid.Parse("d0000000-0000-0000-0000-000000000003");
    public static readonly Guid CampoDeTreinamento = Guid.Parse("d0000000-0000-0000-0000-000000000004");
    public static readonly Guid Biblioteca = Guid.Parse("d0000000-0000-0000-0000-000000000007");
    public static readonly Guid Memorial = Guid.Parse("d0000000-0000-0000-0000-000000000013");
    public static readonly Guid CentroLogistico = Guid.Parse("d0000000-0000-0000-0000-000000000014");
    public static readonly Guid CamaraDoConselho = Guid.Parse("d0000000-0000-0000-0000-000000000017");

    // Doctrines (d1000000-…) — only the two that affect institutional stats.
    public static readonly Guid DoctrineLogistica = Guid.Parse("d1000000-0000-0000-0000-000000000007");
    public static readonly Guid DoctrineComercial = Guid.Parse("d1000000-0000-0000-0000-000000000003");
}
