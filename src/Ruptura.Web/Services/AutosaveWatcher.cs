namespace Ruptura.Web.Services;

// Shared by any page that shows an inline "Saving… / Saved at HH:mm / couldn't save" status next
// to its manual Save button.
public enum AutosaveStatus { Idle, Saving, Saved, Error }

// Polls a snapshot-producing function on an interval and invokes a save callback only when the
// snapshot actually changed since the last successful save. This is the practical form of
// "save automatically ~1-2s after the last edit" that's achievable WITHOUT wiring an OnChanged
// event through every nested tab component — character sheets have ~10 tabs, guild sheets ~13,
// and threading a change callback through all of them just for autosave would balloon a small
// feature into a sprawling cross-cutting edit. A page that owns a mutable data blob (edited
// in-place by its child tab components) can instead just ask "did the serialized blob change?"
// on a timer — same user-visible effect, far smaller surface area.
public sealed class AutosaveWatcher : IDisposable
{
    private readonly Func<string> _snapshot;
    private readonly Func<Task> _onDirty;
    private readonly PeriodicTimer _timer;
    private readonly CancellationTokenSource _cts = new();
    private string _lastSaved;
    private Task? _loop;

    public AutosaveWatcher(TimeSpan interval, Func<string> snapshot, Func<Task> onDirty)
    {
        _snapshot = snapshot;
        _onDirty = onDirty;
        _timer = new PeriodicTimer(interval);
        _lastSaved = snapshot();
    }

    public void Start() => _loop ??= RunAsync(_cts.Token);

    // Call after ANY successful save (autosave or the manual button) so the next tick doesn't
    // re-fire on data that's already persisted.
    public void MarkSaved() => _lastSaved = _snapshot();

    // Call to make the watcher re-check against a specific baseline (e.g. after a conflict
    // reload replaced the in-memory data with the server's copy) without going through a save.
    public void ResetBaseline() => _lastSaved = _snapshot();

    private async Task RunAsync(CancellationToken ct)
    {
        try
        {
            while (await _timer.WaitForNextTickAsync(ct))
            {
                var current = _snapshot();
                if (current == _lastSaved) continue;

                try
                {
                    await _onDirty();
                }
                catch (Exception) when (!ct.IsCancellationRequested)
                {
                    // A single tick failing (e.g. the network drops mid-request) must not kill
                    // the loop — that would silently stop all future autosaving for the rest of
                    // the page's lifetime with no way to recover short of a reload. _onDirty is
                    // expected to record its own failure state (e.g. an "autosave failed" status)
                    // before rethrowing/returning; we just need the polling itself to survive.
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Dispose() was called — normal shutdown, not an error.
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _timer.Dispose();
        _cts.Dispose();
    }
}
