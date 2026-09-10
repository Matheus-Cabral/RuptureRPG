namespace Ruptura.Shared.CharacterSheets;

// GDD §6.4/§6.5 — the 11 Área de Perícia values SkillCatalogData.Area actually uses (the
// seeded catalog treats the 4 Combate sub-areas as distinct, unlike §6.5's table row which
// groups them into one line). Reused by: the Guild Staff tab's "dedicate an Instrutor" Área
// picker (server-side dedication validation) and, from TrainingCalculator (Task 2), the
// Área→Instalação bonus mapping.
public static class TrainingReference
{
    public static readonly IReadOnlyList<string> AreaNames =
    [
        "Combate — Armas", "Combate — Defesa", "Combate Corporal", "Combate à Distância",
        "Exploração", "Conhecimento", "Cura", "Artesanato", "Alquimia", "Magia", "Social"
    ];
}
