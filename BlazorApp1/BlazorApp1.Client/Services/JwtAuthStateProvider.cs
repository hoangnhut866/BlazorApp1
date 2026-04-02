using System.Security.Claims;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace BlazorApp1.Client.Services;

/// <summary>
/// Custom AuthenticationStateProvider that reads a JWT from Local Storage,
/// parses its claims (including roles), and notifies the app of auth state changes.
/// </summary>
public class JwtAuthStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "authToken";

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    private readonly ILocalStorageService _localStorage;

    public JwtAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync(TokenKey);

        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        try
        {
            var claims = ParseClaimsFromJwt(token);
            // Set nameType = Email, roleType = ClaimTypes.Role so that
            // Identity.Name and IsInRole() work correctly.
            var identity = new ClaimsIdentity(
                claims, "jwt", ClaimTypes.Email, ClaimTypes.Role);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch
        {
            // Corrupted token — treat as anonymous
            await _localStorage.RemoveItemAsync(TokenKey);
            return Anonymous;
        }
    }

    public async Task MarkAsAuthenticatedAsync(string token)
    {
        await _localStorage.SetItemAsStringAsync(TokenKey, token);

        var claims = ParseClaimsFromJwt(token);
        var identity = new ClaimsIdentity(claims, "jwt", ClaimTypes.Email, ClaimTypes.Role);
        var user = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkAsLoggedOutAsync()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    /// <summary>Returns the raw JWT string currently stored, or null if not logged in.</summary>
    public async Task<string?> GetTokenAsync()
        => await _localStorage.GetItemAsStringAsync(TokenKey);

    // ── JWT claim parsing ─────────────────────────────────────────────────────

    private static IEnumerable<Claim> ParseClaimsFromJwt(string jwt)
    {
        var payload = jwt.Split('.')[1];
        var jsonBytes = FromBase64UrlString(payload);

        var claims = new List<Claim>();
        var parsed = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonBytes)
                     ?? [];

        foreach (var (key, value) in parsed)
        {
            if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in value.EnumerateArray())
                    claims.Add(new Claim(key, item.GetString() ?? string.Empty));
            }
            else
            {
                claims.Add(new Claim(key, value.ValueKind == JsonValueKind.String
                    ? (value.GetString() ?? string.Empty)
                    : value.ToString()));
            }
        }

        return claims;
    }

    private static byte[] FromBase64UrlString(string base64Url)
    {
        var s = base64Url.Replace('-', '+').Replace('_', '/');
        s = (s.Length % 4) switch
        {
            2 => s + "==",
            3 => s + "=",
            _ => s
        };
        return Convert.FromBase64String(s);
    }
}
