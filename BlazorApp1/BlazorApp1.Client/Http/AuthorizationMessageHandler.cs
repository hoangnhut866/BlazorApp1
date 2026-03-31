using BlazorApp1.Client.Services;

namespace BlazorApp1.Client.Http;

/// <summary>
/// DelegatingHandler that reads the JWT from Local Storage and attaches it
/// as a Bearer token on every outgoing request to the backend API.
/// </summary>
public class AuthorizationMessageHandler : DelegatingHandler
{
    private readonly JwtAuthStateProvider _authStateProvider;

    public AuthorizationMessageHandler(JwtAuthStateProvider authStateProvider)
    {
        _authStateProvider = authStateProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var token = await _authStateProvider.GetTokenAsync();

        if (!string.IsNullOrWhiteSpace(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
