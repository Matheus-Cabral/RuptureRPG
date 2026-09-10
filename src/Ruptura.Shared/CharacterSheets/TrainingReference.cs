using Ruptura.Shared.Guilds;

namespace Ruptura.Shared.CharacterSheets;

// GDD §6.4/§6.5 — the 11 Área de Perícia values SkillCatalogData.Area actually uses (the
// seeded catalog treats the 4 Combate sub-areas as distinct, unlike §6.5's table row which
// groups them into one line). Reused by: the Guild Staff tab's "dedicate an Instrutor" Área
// picker (server-side dedication validation, Task 1) and TrainingCalculator's installation
// bonus lookup (below).
public static class TrainingReference
{
    public static readonly IReadOnlyList<string> AreaNames =
    [
        "Combate — Armas", "Combate — Defesa", "Combate Corporal", "Combate à Distância",
        "Exploração", "Conhecimento", "Cura", "Artesanato", "Alquimia", "Magia", "Social"
    ];

    private static readonly TrainingInstallationMapping Combat =
        new(GuildCatalogIds.CampoDeTreinamento, GuildCatalogIds.AcademiaMilitar);

    // Área → installation mapping (GDD §6.5). "Social" is deliberately absent — it has no
    // installation at all in the normal case (handled as a special case, see spec §4/Key
    // Decision #5, alongside the Liderança-only Academia Militar exception, both implemented
    // directly in TrainingCalculator rather than through this table).
    public static readonly IReadOnlyDictionary<string, TrainingInstallationMapping> InstallationByArea =
        new Dictionary<string, TrainingInstallationMapping>(StringComparer.OrdinalIgnoreCase)
        {
            ["Combate — Armas"] = Combat,
            ["Combate — Defesa"] = Combat,
            ["Combate Corporal"] = Combat,
            ["Combate à Distância"] = Combat,
            ["Exploração"] = new TrainingInstallationMapping(GuildCatalogIds.CampoDeTreinamento, null, HalvedAgain: true),
            ["Conhecimento"] = new TrainingInstallationMapping(GuildCatalogIds.Biblioteca, null),
            ["Cura"] = new TrainingInstallationMapping(GuildCatalogIds.Enfermaria, null),
            ["Artesanato"] = new TrainingInstallationMapping(GuildCatalogIds.Oficina, null),
            ["Alquimia"] = new TrainingInstallationMapping(GuildCatalogIds.JardimAlquimico, null, FallbackId: GuildCatalogIds.Oficina),
            ["Magia"] = new TrainingInstallationMapping(GuildCatalogIds.LaboratorioArcano, GuildCatalogIds.TorreDosMagos)
        };
}

// NormalId: installation whose Level×0.5 is the normal Bônus de Instalação. AdvancedId: the
// "avançada" installation (Level×1 instead of ×0.5) when built and active — takes priority
// over NormalId when present. HalvedAgain: Exploração's GDD rule (Level×0.25 off NormalId).
// FallbackId: Alquimia's "Oficina, se Jardim Alquímico ainda não construído" rule — used only
// when NormalId resolves to Level 0.
public readonly record struct TrainingInstallationMapping(
    Guid NormalId, Guid? AdvancedId, bool HalvedAgain = false, Guid? FallbackId = null);
