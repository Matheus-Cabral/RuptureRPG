using System.Globalization;
using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Ruptura.Web;
using Ruptura.Web.Auth;
using Ruptura.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Load runtime config (ApiBaseUrl) injected by nginx entrypoint via envsubst
var bootstrapHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var appConfig = await bootstrapHttp.GetFromJsonAsync<AppConfig>("config.json") ?? new AppConfig();
builder.Services.AddSingleton(appConfig);

// Localization
builder.Services.AddLocalization();

// Auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

// Services
builder.Services.AddScoped<IAuthClientService, AuthClientService>();
builder.Services.AddScoped<LanguageService>();

// Named HttpClient that sends Accept-Language header automatically
builder.Services.AddScoped(sp =>
{
    var culture = CultureInfo.CurrentUICulture.Name;
    var client = new HttpClient { BaseAddress = new Uri(appConfig.ApiBaseUrl) };
    client.DefaultRequestHeaders.AcceptLanguage.ParseAdd(culture);
    return client;
});

var host = builder.Build();

// Apply stored culture BEFORE running the app
var storage = host.Services.GetRequiredService<ILocalStorageService>();
var stored = await storage.GetItemAsync<string>("ruptura_culture") ?? "en";
var ci = new CultureInfo(stored);
CultureInfo.DefaultThreadCurrentCulture = ci;
CultureInfo.DefaultThreadCurrentUICulture = ci;

await host.RunAsync();
