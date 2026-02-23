using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace arna.HRMS.ClientServices.Auth; 

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string UserIdKey = "auth_user_id";

    private const string AuthType = "jwt";

    private readonly ProtectedLocalStorage _localStorage;
    private string? _cachedToken;
    private ClaimsPrincipal _cachedUser =
        new(new ClaimsIdentity());
    private static AuthenticationState Anonymous =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(ProtectedLocalStorage localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (_cachedToken != null)
            return new AuthenticationState(_cachedUser);

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

        var now = DateTime.UtcNow;
        if (jwt.ValidTo < now.AddMinutes(-1))
            return Anonymous;

        var identity = new ClaimsIdentity(jwt.Claims, AuthType);
        var user = new ClaimsPrincipal(identity);

        _cachedToken = accessToken;
        _cachedUser = user;

        return new AuthenticationState(user);
    }

    public async Task LoginAsync(int userId, string accessToken, string refreshToken)
    {
        await _localStorage.SetAsync(UserIdKey, userId);
        await _localStorage.SetAsync(AccessTokenKey, accessToken);
        await _localStorage.SetAsync(RefreshTokenKey, refreshToken);

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

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task UpdateTokensAsync(string newAccessToken, string newRefreshToken)
    {
        await _localStorage.SetAsync(AccessTokenKey, newAccessToken);
        await _localStorage.SetAsync(RefreshTokenKey, newRefreshToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    private async Task ClearAuthAsync()
    {
        await _localStorage.DeleteAsync(UserIdKey);
        await _localStorage.DeleteAsync(AccessTokenKey);
        await _localStorage.DeleteAsync(RefreshTokenKey);
    }
}
