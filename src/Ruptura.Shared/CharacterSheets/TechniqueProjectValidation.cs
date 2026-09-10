namespace Ruptura.Shared.CharacterSheets;

// Result of a read-only "can this project start?" check. CanStart=false + a BlockedReason is
// a normal domain answer, not an error — the endpoint still returns 200. RequiredDays is
// always populated (display-only when CanStart=false, and what the client copies onto the new
// CharacterTechniqueProject when CanStart=true).
public class TechniqueProjectValidation
{
    public bool CanStart { get; set; }
    public string? BlockedReason { get; set; } // "InsufficientSkill" | "InsufficientRanking" | "MissingInstallation" | null
    public int RequiredDays { get; set; }
}
