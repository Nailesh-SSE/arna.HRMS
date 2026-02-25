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

    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    private ClaimsPrincipal _cachedUser = new(new ClaimsIdentity());
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
        try
        {
            var accessToken = await GetAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(accessToken))
                return Anonymous;

            JwtSecurityToken jwt;
            try
            {
                jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to read JWT token");
                 ClearAuthAsync();
                return Anonymous;
            }

            var now = DateTime.UtcNow;
            if (jwt.ValidTo < now.AddMinutes(-1))
            {
                 ClearAuthAsync();
                return Anonymous;
            }

            var identity = new ClaimsIdentity(jwt.Claims, AuthType);
            var user = new ClaimsPrincipal(identity);

            _cachedUser = user;

            return new AuthenticationState(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetAuthenticationStateAsync");
            return Anonymous;
        }
    }

    public Task LoginAsync(int userId, string accessToken, string refreshToken)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
            var expiry = jwt.ValidTo;

            // Store tokens in memory cache with appropriate expiration
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiry)
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetPriority(CacheItemPriority.High);

            var refreshOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(7))
                .SetPriority(CacheItemPriority.High);

            _memoryCache.Set(AccessTokenKey, accessToken, cacheEntryOptions);
            _memoryCache.Set(RefreshTokenKey, refreshToken, refreshOptions);
            _memoryCache.Set(UserIdKey, userId, refreshOptions);

            var identity = new ClaimsIdentity(jwt.Claims, AuthType);
            _cachedUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            _logger.LogInformation("User {UserId} logged in successfully", userId);

            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            throw;
        }
    }

    public Task LogoutAsync()
    {
        ClearAuthAsync();
        _cachedUser = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));

        _logger.LogInformation("User logged out");

        return Task.CompletedTask;
    }

    public Task<string?> GetAccessTokenAsync()
    {
        try
        {
            if (_memoryCache.TryGetValue(AccessTokenKey, out string? token))
                return Task.FromResult(token);

            return Task.FromResult<string?>(null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting access token");
            return Task.FromResult<string?>(null);
        }
    }

    public Task<(int userId, string? refreshToken)> GetRefreshDataAsync()
    {
        try
        {
            _memoryCache.TryGetValue(UserIdKey, out int userId);
            _memoryCache.TryGetValue(RefreshTokenKey, out string? refreshToken);

            return Task.FromResult((userId, refreshToken));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting refresh data");
            return null;
        }
    }

    public Task UpdateAccessTokenAsync(string newAccessToken)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(newAccessToken);
            var expiry = jwt.ValidTo;

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiry)
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetPriority(CacheItemPriority.High);

            _memoryCache.Set(AccessTokenKey, newAccessToken, cacheEntryOptions);

            var identity = new ClaimsIdentity(jwt.Claims, AuthType);
            _cachedUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            _logger.LogDebug("Access token updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating access token");
        }

        return Task.CompletedTask;
    }

    public Task UpdateTokensAsync(string newAccessToken, string newRefreshToken)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(newAccessToken);
            var expiry = jwt.ValidTo;

            var tokenOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(expiry)
                .SetSlidingExpiration(TimeSpan.FromMinutes(30))
                .SetPriority(CacheItemPriority.High);

            var refreshOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromDays(7))
                .SetPriority(CacheItemPriority.High);

            _memoryCache.Set(AccessTokenKey, newAccessToken, tokenOptions);
            _memoryCache.Set(RefreshTokenKey, newRefreshToken, refreshOptions);

            var identity = new ClaimsIdentity(jwt.Claims, AuthType);
            _cachedUser = new ClaimsPrincipal(identity);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

            _logger.LogDebug("Tokens updated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating tokens");
        }

        return Task.CompletedTask;
    }

    private void ClearAuthAsync()
    {
        try
        {
            _memoryCache.Remove(AccessTokenKey);
            _memoryCache.Remove(RefreshTokenKey);
            _memoryCache.Remove(UserIdKey);

            _logger.LogDebug("Auth cache cleared");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing auth cache");
        }
    }
}