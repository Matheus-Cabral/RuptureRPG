namespace Ruptura.Shared.Content;

// The GM's arc prep inputs, persisted as the Arc.DataJson blob. String-only so
// Ruptura.Shared carries no project references.
public class ArcData
{
    public string Theme { get; set; } = string.Empty;
    public string History { get; set; } = string.Empty;
    public string Conflict { get; set; } = string.Empty;
    public string FinalObjective { get; set; } = string.Empty;
    public string Ecosystem { get; set; } = string.Empty;
    public string Resources { get; set; } = string.Empty;
    public string Mechanic { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}

// The GM's floor prep inputs, persisted as the Floor.DataJson blob. ObjectiveType is a
// string keyed against ContentReference.ObjectiveTypes.
public class FloorData
{
    // Key from ContentReference.ObjectiveTypes.
    public string ObjectiveType { get; set; } = string.Empty;
    public string Identity { get; set; } = string.Empty;
    public string MainObjective { get; set; } = string.Empty;
    public List<string> SecondaryObjectives { get; set; } = [];
    public string FailureCondition { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;

    // Soft links to Encounter/Reward entities in the same campaign; resolved to names at read time.
    public List<Guid> LinkedEncounterIds { get; set; } = [];
    public List<Guid> LinkedRewardIds { get; set; } = [];
}

// The GM's session log inputs, persisted as the SessionLog.DataJson blob.
public class SessionLogData
{
    public string Recap { get; set; } = string.Empty;
    public string Agenda { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
