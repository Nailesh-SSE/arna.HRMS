using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace arna.HRMS.Services.Auth;

public sealed class CustomAuthStateProvider : AuthenticationStateProvider
{
    private const string AccessTokenKey = "auth_access_token";
    private const string RefreshTokenKey = "auth_refresh_token";
    private const string UserIdKey = "auth_user_id";
    private const string AuthType = "jwt";

    private readonly ProtectedLocalStorage _protectedLocalStorage;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(
        ProtectedLocalStorage protectedLocalStorage,
        ILogger<CustomAuthStateProvider> logger)
    {
        _protectedLocalStorage = protectedLocalStorage;
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
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

            await _protectedLocalStorage.SetAsync(AccessTokenKey, accessToken);
            await _protectedLocalStorage.SetAsync(RefreshTokenKey, refreshToken);
            await _protectedLocalStorage.SetAsync(UserIdKey, userId);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to store authentication tokens");
            throw;
        }
    }

    public async Task UpdateTokensAsync(string accessToken, string refreshToken)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

            await _protectedLocalStorage.SetAsync(AccessTokenKey, accessToken);
            await _protectedLocalStorage.SetAsync(RefreshTokenKey, refreshToken);

            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update authentication tokens");
            throw;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _protectedLocalStorage.DeleteAsync(AccessTokenKey);
            await _protectedLocalStorage.DeleteAsync(RefreshTokenKey);
            await _protectedLocalStorage.DeleteAsync(UserIdKey);

            NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to clear authentication tokens");
        }
    }

    public async Task<int> GetUserIdAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<int>(UserIdKey);
            return result.Success ? result.Value : 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user ID");
            return 0;
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<string>(AccessTokenKey);
            return result.Success ? result.Value : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve access token");
            return null;
        }
    }

    public async Task<(int userId, string? refreshToken)> GetRefreshDataAsync()
    {
        try
        {
            var userIdResult = await _protectedLocalStorage.GetAsync<int>(UserIdKey);
            var refreshTokenResult = await _protectedLocalStorage.GetAsync<string>(RefreshTokenKey);

            var userId = userIdResult.Success ? userIdResult.Value : 0;
            var refreshToken = refreshTokenResult.Success ? refreshTokenResult.Value : null;

            return (userId, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve refresh data");
            return (0, null);
        }
    }
}