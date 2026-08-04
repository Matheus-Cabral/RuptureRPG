using Blazored.LocalStorage;
using Microsoft.JSInterop;

namespace Ruptura.Web.Services;

public enum ThemeMode { Light, Dark, System }

public class ThemeService(ILocalStorageService storage, IJSRuntime js)
{
    private const string Key = "ruptura_theme";

    public ThemeMode Current { get; private set; } = ThemeMode.System;

    public async Task InitAsync()
    {
        var stored = await storage.GetItemAsync<string>(Key) ?? "system";
        Current = stored switch
        {
            "light"  => ThemeMode.Light,
            "dark"   => ThemeMode.Dark,
            _        => ThemeMode.System
        };
        await ApplyAsync();
    }

    public async Task SetAsync(ThemeMode mode)
    {
        Current = mode;
        await storage.SetItemAsync(Key, mode.ToString().ToLower());
        await ApplyAsync();
    }

    private Task ApplyAsync() =>
        js.InvokeVoidAsync("ruptura.setTheme", Current.ToString().ToLower()).AsTask();
}
