namespace Ruptura.Web.Services;

public enum ToastLevel { Success, Error, Info }

public sealed record ToastMessage(Guid Id, string Text, ToastLevel Level);

public class ToastService
{
    private readonly List<ToastMessage> _messages = [];

    public event Action? OnChange;

    public IReadOnlyList<ToastMessage> Messages => _messages;

    public void Show(string text, ToastLevel level = ToastLevel.Info)
    {
        _messages.Add(new ToastMessage(Guid.NewGuid(), text, level));
        OnChange?.Invoke();
    }

    public void Success(string text) => Show(text, ToastLevel.Success);

    public void Error(string text) => Show(text, ToastLevel.Error);

    public void Dismiss(Guid id)
    {
        _messages.RemoveAll(m => m.Id == id);
        OnChange?.Invoke();
    }
}
