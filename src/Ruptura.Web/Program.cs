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

// Load runtime config injected by nginx entrypoint
var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
var appConfig = await http.GetFromJsonAsync<AppConfig>("config.json") ?? new AppConfig();
builder.Services.AddSingleton(appConfig);

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, JwtAuthStateProvider>();
builder.Services.AddScoped<IAuthClientService, AuthClientService>();

builder.Services.AddScoped(sp =>
{
    var token = sp.GetRequiredService<ILocalStorageService>();
    var client = new HttpClient { BaseAddress = new Uri(appConfig.ApiBaseUrl) };
    return client;
});

await builder.Build().RunAsync();
