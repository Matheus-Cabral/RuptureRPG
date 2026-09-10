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
    public static readonly Guid Oficina = Guid.Parse("d0000000-0000-0000-0000-000000000006");
    public static readonly Guid OficinaDeRunas = Guid.Parse("d0000000-0000-0000-0000-000000000012");
    public static readonly Guid Enfermaria = Guid.Parse("d0000000-0000-0000-0000-000000000008");
    public static readonly Guid LaboratorioArcano = Guid.Parse("d0000000-0000-0000-0000-000000000009");
    public static readonly Guid AcademiaMilitar = Guid.Parse("d0000000-0000-0000-0000-000000000010");
    public static readonly Guid JardimAlquimico = Guid.Parse("d0000000-0000-0000-0000-000000000011");
    public static readonly Guid TorreDosMagos = Guid.Parse("d0000000-0000-0000-0000-000000000016");
    public static readonly Guid Memorial = Guid.Parse("d0000000-0000-0000-0000-000000000013");
    public static readonly Guid CentroLogistico = Guid.Parse("d0000000-0000-0000-0000-000000000014");
    public static readonly Guid CamaraDoConselho = Guid.Parse("d0000000-0000-0000-0000-000000000017");

    // Doctrines (d1000000-…) — only the two that affect institutional stats.
    public static readonly Guid DoctrineLogistica = Guid.Parse("d1000000-0000-0000-0000-000000000007");
    public static readonly Guid DoctrineComercial = Guid.Parse("d1000000-0000-0000-0000-000000000003");
}
