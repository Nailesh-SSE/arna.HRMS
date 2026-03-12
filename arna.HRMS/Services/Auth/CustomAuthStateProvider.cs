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

    private readonly ProtectedSessionStorage _sessionStorage;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    // Cache — once loaded, never hits storage again until login/logout
    private AuthenticationState? _cachedState;

    // Tracks whether JS interop is available yet
    private bool _isReady;

    private static readonly AuthenticationState Anonymous =
        new(new ClaimsPrincipal(new ClaimsIdentity()));

    public CustomAuthStateProvider(
        ProtectedSessionStorage sessionStorage,
        ILogger<CustomAuthStateProvider> logger)
    {
        _sessionStorage = sessionStorage;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        // Return cached state immediately — no storage hit
        if (_cachedState is not null)
            return _cachedState;

        // If not ready (JS not connected yet), return Anonymous immediately.
        // This prevents the <Authorizing> block from hanging.
        // Blazor will call this again after the circuit connects.
        if (!_isReady)
            return Anonymous;

        _cachedState = await BuildStateFromStorageAsync();
        return _cachedState;
    }

    // Call this once after the component has rendered (JS is ready)
    // Call it from your main layout or App.razor OnAfterRenderAsync
    public async Task InitializeAsync()
    {
        if (_isReady) return;

        _isReady = true;
        _cachedState = await BuildStateFromStorageAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
    }

    public async Task LoginAsync(int userId, string accessToken, string refreshToken)
    {
        _isReady = true;
        await _sessionStorage.SetAsync(AccessTokenKey, accessToken);
        await _sessionStorage.SetAsync(RefreshTokenKey, refreshToken);
        await _sessionStorage.SetAsync(UserIdKey, userId);

        _cachedState = BuildStateFromToken(accessToken);
        NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
    }

    public async Task LogoutAsync()
    {
        try { await _sessionStorage.DeleteAsync(AccessTokenKey); } catch { }
        try { await _sessionStorage.DeleteAsync(RefreshTokenKey); } catch { }
        try { await _sessionStorage.DeleteAsync(UserIdKey); } catch { }

        _cachedState = Anonymous;
        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    public async Task UpdateTokensAsync(string newAccessToken, string newRefreshToken)
    {
        await _sessionStorage.SetAsync(AccessTokenKey, newAccessToken);
        await _sessionStorage.SetAsync(RefreshTokenKey, newRefreshToken);

        _cachedState = BuildStateFromToken(newAccessToken);
        NotifyAuthenticationStateChanged(Task.FromResult(_cachedState));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            var result = await _sessionStorage.GetAsync<string>(AccessTokenKey);
            return result.Success ? result.Value : null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read access token");
            return null;
        }
    }

    public async Task<(int userId, string? refreshToken)> GetRefreshDataAsync()
    {
        try
        {
            var userIdResult = await _sessionStorage.GetAsync<int>(UserIdKey);
            var refreshTokenResult = await _sessionStorage.GetAsync<string>(RefreshTokenKey);
            return (
                userIdResult.Success ? userIdResult.Value : 0,
                refreshTokenResult.Success ? refreshTokenResult.Value : null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not read refresh data");
            return (0, null);
        }
    }

    private async Task<AuthenticationState> BuildStateFromStorageAsync()
    {
        string? token = null;

        try
        {
            var result = await _sessionStorage.GetAsync<string>(AccessTokenKey);
            token = result.Success ? result.Value : null;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Session storage read failed");
            return Anonymous;
        }

        if (string.IsNullOrWhiteSpace(token))
            return Anonymous;

        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);

            if (jwt.ValidTo <= DateTime.UtcNow)
            {
                await LogoutAsync();
                return Anonymous;
            }

            return BuildStateFromToken(token);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Invalid JWT token in storage");
            await LogoutAsync();
            return Anonymous;
        }
    }

    private static AuthenticationState BuildStateFromToken(string accessToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);
        var identity = new ClaimsIdentity(jwt.Claims, AuthType);
        return new AuthenticationState(new ClaimsPrincipal(identity));
    }
}