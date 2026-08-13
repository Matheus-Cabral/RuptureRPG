namespace Ruptura.Shared.Content;

// Single source of truth for the session-prep content fixed picker sets. Consumed by the
// write-side validators and the UI pickers. String-only to keep Ruptura.Shared free of
// project references.
public static class ContentReference
{
    // Floor objective types (FloorData.ObjectiveType).
    public static readonly IReadOnlyList<string> ObjectiveTypes =
    [
        "Exploracao", "Reconhecimento", "Defesa", "Ataque", "Caca",
        "Escolta", "Sobrevivencia", "Puzzle", "Eliminacao"
    ];
}
