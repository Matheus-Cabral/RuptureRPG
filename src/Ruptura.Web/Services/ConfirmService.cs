namespace Ruptura.Web.Services;

public sealed record ConfirmRequest(string Title, string Message, string ConfirmLabel, string CancelLabel);

public class ConfirmService
{
    private TaskCompletionSource<bool>? _pending;

    public event Action? OnChange;

    public ConfirmRequest? Current { get; private set; }

    public Task<bool> AskAsync(string title, string message, string confirmLabel, string cancelLabel)
    {
        _pending?.TrySetResult(false); // an unresolved prior request is treated as cancelled
        _pending = new TaskCompletionSource<bool>();
        Current = new ConfirmRequest(title, message, confirmLabel, cancelLabel);
        OnChange?.Invoke();
        return _pending.Task;
    }

    public void Resolve(bool result)
    {
        var pending = _pending;
        Current = null;
        _pending = null;
        OnChange?.Invoke();
        pending?.TrySetResult(result);
    }
}
