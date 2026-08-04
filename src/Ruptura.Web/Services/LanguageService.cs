using System.Globalization;
using Blazored.LocalStorage;

namespace Ruptura.Web.Services;

public class LanguageService(ILocalStorageService localStorage)
{
    private const string Key = "ruptura_culture";
    public static readonly string[] Supported = ["en", "pt-BR"];
    public string Current => CultureInfo.CurrentUICulture.Name;

    public async Task<string> GetStoredCultureAsync() =>
        await localStorage.GetItemAsync<string>(Key) ?? "en";

    public async Task SetCultureAsync(string culture)
    {
        if (!Supported.Contains(culture)) culture = "en";
        await localStorage.SetItemAsync(Key, culture);
        // Requires page reload to take effect in Blazor WASM
        var uri = new Uri(NavigationManagerUri);
        await localStorage.SetItemAsync(Key, culture);
    }

    // Called at app startup (in Program.cs) before RunAsync
    public static async Task ApplyStoredCultureAsync(ILocalStorageService storage)
    {
        var culture = await storage.GetItemAsync<string>(Key) ?? "en";
        var ci = new CultureInfo(culture);
        CultureInfo.DefaultThreadCurrentCulture = ci;
        CultureInfo.DefaultThreadCurrentUICulture = ci;
    }

    // NavigationManager URI — injected separately at call site
    public string NavigationManagerUri { get; set; } = "/";
}
