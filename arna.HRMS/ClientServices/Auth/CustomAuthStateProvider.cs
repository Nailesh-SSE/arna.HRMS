using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace arna.HRMS.ClientServices.Auth; 

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string UserIdKey = "auth_user_id";

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
        var accessToken = await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(accessToken))
            return Anonymous;

        JwtSecurityToken jwt;
        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        }
        catch
        {
            await ClearAuthAsync();
            return Anonymous;
        }

        if (jwt.ValidTo <= DateTime.UtcNow)
        {
            return Anonymous;
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        return new AuthenticationState(
            new ClaimsPrincipal(
                new ClaimsIdentity(jwt.Claims, AuthType)));
    }

    public async Task LoginAsync(int userId, string accessToken, string refreshToken)
    {
        await _localStorage.SetAsync(UserIdKey, userId);
        await _localStorage.SetAsync(AccessTokenKey, accessToken);
        await _localStorage.SetAsync(RefreshTokenKey, refreshToken);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        await ClearAuthAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var result = await _localStorage.GetAsync<string>(AccessTokenKey);
            return result.Success ? result.Value : null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<(int userId, string? refreshToken)> GetRefreshDataAsync()
    {
        try
        {
            var userIdResult = await _localStorage.GetAsync<int>(UserIdKey);
            var refreshResult = await _localStorage.GetAsync<string>(RefreshTokenKey);

            var userId = userIdResult.Success ? userIdResult.Value : 0;
            var refreshToken = refreshResult.Success ? refreshResult.Value : null;

            return (userId, refreshToken);
        }
        catch
        {
            return (0, null);
        }
    }

    public async Task UpdateAccessTokenAsync(string newAccessToken)
    {
        await _localStorage.SetAsync(AccessTokenKey, newAccessToken);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", newAccessToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task UpdateTokensAsync(string newAccessToken, string newRefreshToken)
    {
        await _localStorage.SetAsync(AccessTokenKey, newAccessToken);
        await _localStorage.SetAsync(RefreshTokenKey, newRefreshToken);

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", newAccessToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task ClearAuthAsync()
    {
        await _localStorage.DeleteAsync(UserIdKey);
        await _localStorage.DeleteAsync(AccessTokenKey);
        await _localStorage.DeleteAsync(RefreshTokenKey);

        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
