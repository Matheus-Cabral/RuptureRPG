namespace Ruptura.Web.Services;

// Mirrors ConfirmService: a scoped, event-driven holder for the app-wide image lightbox.
public class LightboxService
{
    public event Action? OnChange;
    public string? CurrentImage { get; private set; }
    public string? CurrentAlt { get; private set; }

    public void Show(string dataUri, string? alt = null)
    {
        CurrentImage = dataUri;
        CurrentAlt = alt;
        OnChange?.Invoke();
    }

    public void Close()
    {
        CurrentImage = null;
        CurrentAlt = null;
        OnChange?.Invoke();
    }
}
