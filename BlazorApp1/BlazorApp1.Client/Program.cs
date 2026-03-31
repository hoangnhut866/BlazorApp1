using Blazored.LocalStorage;
using BlazorApp1.Client;
using BlazorApp1.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ─── HTTP Client ──────────────────────────────────────────────────────────────
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("'ApiBaseUrl' is not set in appsettings.json.");

builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// ─── Local Storage ────────────────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();

// ─── Authentication & Authorization ──────────────────────────────────────────
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

// Register JwtAuthStateProvider both as itself (for LoginAsync to call) and
// as the AuthenticationStateProvider used by the Blazor auth infrastructure.
builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthStateProvider>());

// ─── App Services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();

await builder.Build().RunAsync();
