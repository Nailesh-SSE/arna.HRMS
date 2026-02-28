using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Caching.Memory;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace arna.HRMS.Services.Auth;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string UserIdKey = "auth_user_id";
    private const string AuthType = "jwt";

    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(
        IMemoryCache memoryCache,
        ILogger<CustomAuthStateProvider> logger)
    {
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var token = await GetAccessTokenAsync();

        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            if (jwt.ValidTo <= DateTime.UtcNow.AddMinutes(-1))
            {
                await LogoutAsync();
                return Anonymous;
            }

            var identity = new ClaimsIdentity(jwt.Claims, AuthType);
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid JWT token");
            await LogoutAsync();
            return Anonymous;
        }
    }

    public async Task LoginAsync(int userId, string accessToken, string refreshToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var expiry = jwt.ValidTo;

        _memoryCache.Set(AccessTokenKey, accessToken,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiry));

        _memoryCache.Set(RefreshTokenKey, refreshToken,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(7)));

        _memoryCache.Set(UserIdKey, userId);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        await Task.CompletedTask;
    }

    public async Task LogoutAsync()
    {
        _memoryCache.Remove(AccessTokenKey);
        _memoryCache.Remove(RefreshTokenKey);
        _memoryCache.Remove(UserIdKey);

        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));

        await Task.CompletedTask;
    }

    public Task<string?> GetAccessTokenAsync()
    {
        _memoryCache.TryGetValue(AccessTokenKey, out string? token);
        return Task.FromResult(token);
    }

    public Task<(int userId, string? refreshToken)> GetRefreshDataAsync()
    {
        _memoryCache.TryGetValue(UserIdKey, out int userId);
        _memoryCache.TryGetValue(RefreshTokenKey, out string? refreshToken);

        return Task.FromResult((userId, refreshToken));
    }

    public async Task UpdateTokensAsync(string newAccessToken, string newRefreshToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(newAccessToken);
        var expiry = jwt.ValidTo;

        _memoryCache.Set(AccessTokenKey, newAccessToken,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(expiry));

        _memoryCache.Set(RefreshTokenKey, newRefreshToken,
            new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromDays(7)));

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

        await Task.CompletedTask;
    }
}