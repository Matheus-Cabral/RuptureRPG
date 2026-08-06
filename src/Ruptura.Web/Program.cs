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

// Runtime config (ApiBaseUrl replaced by nginx entrypoint via envsubst)
var bootstrapHttp = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var appConfig = await bootstrapHttp.GetFromJsonAsync<AppConfig>("config.json") ?? new AppConfig();
builder.Services.AddSingleton(appConfig);

// Localization
builder.Services.AddLocalization();

// Storage + Auth
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();

// Theme
builder.Services.AddScoped<ThemeService>();
builder.Services.AddScoped<ToastService>();

// HTTP client with JWT handler
builder.Services.AddTransient<JwtAuthorizationHandler>();
builder.Services.AddHttpClient("RupturaApi", client =>
    client.BaseAddress = new Uri(appConfig.ApiBaseUrl))
    .AddHttpMessageHandler<JwtAuthorizationHandler>();

// Application services
builder.Services.AddScoped<IAuthClientService, AuthClientService>();
builder.Services.AddScoped<IInviteClientService, InviteClientService>();
builder.Services.AddScoped<ICampaignClientService, CampaignClientService>();
builder.Services.AddScoped<ICatalogClientService, CatalogClientService>();
builder.Services.AddScoped<ICharacterSheetClientService, CharacterSheetClientService>();
builder.Services.AddScoped<IJournalEntryClientService, JournalEntryClientService>();
builder.Services.AddScoped<IMediaClientService, MediaClientService>();
builder.Services.AddScoped<INotificationClientService, NotificationClientService>();

var host = builder.Build();

// Apply stored culture before running
var storage = host.Services.GetRequiredService<ILocalStorageService>();
var culture = await storage.GetItemAsync<string>("ruptura_culture") ?? "en";
var ci = new CultureInfo(culture);
CultureInfo.DefaultThreadCurrentCulture = ci;
CultureInfo.DefaultThreadCurrentUICulture = ci;

await host.RunAsync();
