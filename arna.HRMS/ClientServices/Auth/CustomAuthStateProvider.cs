using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace arna.HRMS.ClientServices.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string TokenKey = "auth_token";
    private const string AuthType = "jwt";

    private readonly ProtectedLocalStorage _localStorage;
    private readonly HttpClient _httpClient;

    private static AuthenticationState Anonymous =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(ProtectedLocalStorage localStorage, HttpClient httpClient)
    {
        _localStorage = localStorage;
        _httpClient = httpClient;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        string? token = null;

        try
        {
            var result = await _localStorage.GetAsync<string>(TokenKey);
            token = result.Success ? result.Value : null;
        }
        catch (InvalidOperationException)
        {
            return Anonymous;
        }

        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        JwtSecurityToken jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        }
        catch
        {
            return Anonymous;
        }

        if (jwt.ValidTo <= DateTime.UtcNow)
            return Anonymous;

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        return new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity(jwt.Claims, AuthType)));
    }

    public async Task LoginAsync(string token)
    {
        await _localStorage.SetAsync(TokenKey, token);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        await _localStorage.DeleteAsync(TokenKey);

        _httpClient.DefaultRequestHeaders.Authorization = null;

        NotifyAuthenticationStateChanged(
            Task.FromResult(Anonymous));
    }
}
