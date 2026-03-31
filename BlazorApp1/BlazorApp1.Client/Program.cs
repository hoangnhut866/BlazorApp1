using Blazored.LocalStorage;
using BlazorApp1.Client;
using BlazorApp1.Client.Http;
using BlazorApp1.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// ─── Local Storage ────────────────────────────────────────────────────────────
builder.Services.AddBlazoredLocalStorage();

// ─── Authentication & Authorization ──────────────────────────────────────────
builder.Services.AddAuthorizationCore();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddScoped<JwtAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthStateProvider>());

// ─── HTTP Client with JWT DelegatingHandler ───────────────────────────────────
var apiBaseUrl = builder.Configuration["ApiBaseUrl"]
    ?? throw new InvalidOperationException("'ApiBaseUrl' is not set in appsettings.json.");

// The handler must be Transient so each IHttpClientFactory scope gets its own.
builder.Services.AddTransient<AuthorizationMessageHandler>();

builder.Services.AddHttpClient("ApiClient", client =>
    client.BaseAddress = new Uri(apiBaseUrl))
    .AddHttpMessageHandler<AuthorizationMessageHandler>();

// Provide a plain HttpClient (no JWT) for the auth endpoints (login doesn't need a token).
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl) });

// ─── App Services ─────────────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductService, ProductService>();

await builder.Build().RunAsync();
