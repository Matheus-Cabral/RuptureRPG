namespace Ruptura.Shared.CharacterSheets;

// Result of a read-only "can this project start?" check. CanStart=false + a BlockedReason is
// a normal domain answer, not an error — the endpoint still returns 200. RequiredDays is
// always populated (display-only when CanStart=false, and what the client copies onto the new
// CharacterTechniqueProject when CanStart=true).
//
// This type is shared across MULTIPLE project-start validations (Technique Creation and
// Crafting) rather than owned by either one. Each feature defines its own set of
// BlockedReason values and localizes them under its own "Sheet.<Feature>.Blocked.*" resx
// prefix: Technique Creation uses "InsufficientSkill" | "InsufficientRanking" |
// "MissingInstallation" under Sheet.Technique.Blocked.*; Crafting uses "RecipeNotKnown" |
// "MissingInstallation" under Sheet.Crafting.Blocked.*. Both features happen to use the
// string "MissingInstallation" as a value, but each resolves it under its own resx prefix,
// so there's no actual collision — just don't assume a BlockedReason value maps to a single
// resx key without knowing which feature produced it.
public class TechniqueProjectValidation
{
    public bool CanStart { get; set; }
    public string? BlockedReason { get; set; } // feature-specific value; see class comment above, or null
    public int RequiredDays { get; set; }
}
