namespace Ruptura.Shared.Guilds;

// Pure helper for the guild editor's optimistic-concurrency safety: the blob is "dirty" when the
// current serialized blob or the guild name differs from the last state the server confirmed.
// Used by GuildSheet.razor to decide whether a child-entity refresh may adopt the server's new
// xmin Version (adopting while dirty would mask a concurrent blob save and cause a lost update).
public static class GuildBlobDirtyState
{
    public static bool IsDirty(string currentBlobJson, string baselineBlobJson, string currentName, string baselineName) =>
        !string.Equals(currentBlobJson, baselineBlobJson, StringComparison.Ordinal)
        || !string.Equals(currentName, baselineName, StringComparison.Ordinal);
}
