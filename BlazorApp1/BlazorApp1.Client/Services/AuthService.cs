using System.Net.Http.Json;
using BlazorApp1.Client.Models;

namespace BlazorApp1.Client.Services;

public interface IAuthService
{
    Task<(bool Success, string? Error)> LoginAsync(string email, string password);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private readonly HttpClient _http;
    private readonly JwtAuthStateProvider _authStateProvider;

    public AuthService(HttpClient http, JwtAuthStateProvider authStateProvider)
    {
        _http = http;
        _authStateProvider = authStateProvider;
    }

    public async Task<(bool Success, string? Error)> LoginAsync(string email, string password)
    {
        try
        {
            var response = await _http.PostAsJsonAsync(
                "api/auth/login",
                new LoginRequest { Email = email, Password = password });

            if (!response.IsSuccessStatusCode)
                return (false, "Invalid email or password.");

            var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (result?.Token is null)
                return (false, "Unexpected response from server.");

            await _authStateProvider.MarkAsAuthenticatedAsync(result.Token);
            return (true, null);
        }
        catch (HttpRequestException)
        {
            return (false, "Unable to reach the server. Please try again.");
        }
        catch (Exception ex)
        {
            return (false, $"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task LogoutAsync()
        => await _authStateProvider.MarkAsLoggedOutAsync();
}
