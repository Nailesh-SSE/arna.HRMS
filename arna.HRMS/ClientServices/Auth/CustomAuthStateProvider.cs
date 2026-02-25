using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace arna.HRMS.ClientServices.Auth;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string UserIdKey = "auth_user_id";

    private const string AuthType = "jwt";

    private readonly IMemoryCache _cache;

    private ClaimsPrincipal _cachedUser =
        new(new ClaimsIdentity());

    private static AuthenticationState Anonymous =>
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public string? CurrentAccessToken =>
        _cache.TryGetValue(AccessTokenKey, out string? token) ? token : null;

    public CustomAuthStateProvider(IMemoryCache cache)
    {
        _cache = cache;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        if (!_cache.TryGetValue(AccessTokenKey, out string? accessToken) ||
            string.IsNullOrWhiteSpace(accessToken))
        {
            return Task.FromResult(Anonymous);
        }

        JwtSecurityToken jwt;

        try
        {
            jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        }
        catch
        {
            ClearAuth();
            return Task.FromResult(Anonymous);
        }

        if (jwt.ValidTo < DateTime.UtcNow)
        {
            ClearAuth();
            return Task.FromResult(Anonymous);
        }

        var identity = new ClaimsIdentity(jwt.Claims, AuthType);
        var user = new ClaimsPrincipal(identity);

        _cachedUser = user;

        return Task.FromResult(new AuthenticationState(user));
    }

    public Task LoginAsync(int userId, string accessToken, string refreshToken)
    {
        _cache.Set(UserIdKey, userId);
        _cache.Set(AccessTokenKey, accessToken);
        _cache.Set(RefreshTokenKey, refreshToken);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var identity = new ClaimsIdentity(jwt.Claims, AuthType);
        _cachedUser = new ClaimsPrincipal(identity);

        NotifyAuthenticationStateChanged(
            Task.FromResult(new AuthenticationState(_cachedUser)));

        return Task.CompletedTask;
    }

    public Task LogoutAsync()
    {
        ClearAuth();

        NotifyAuthenticationStateChanged(
            Task.FromResult(Anonymous));

        return Task.CompletedTask;
    }

    public Task<string?> GetAccessTokenAsync()
    {
        _cache.TryGetValue(AccessTokenKey, out string? token);
        return Task.FromResult(token);
    }

    public Task<(int userId, string? refreshToken)> GetRefreshDataAsync()
    {
        _cache.TryGetValue(UserIdKey, out int userId);
        _cache.TryGetValue(RefreshTokenKey, out string? refreshToken);

        return Task.FromResult((userId, refreshToken));
    }

    public Task UpdateTokensAsync(string newAccessToken, string newRefreshToken)
    {
        _cache.Set(AccessTokenKey, newAccessToken);
        _cache.Set(RefreshTokenKey, newRefreshToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        return Task.CompletedTask;
    }

    private void ClearAuth()
    {
        _cache.Remove(UserIdKey);
        _cache.Remove(AccessTokenKey);
        _cache.Remove(RefreshTokenKey);

        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
    }
}