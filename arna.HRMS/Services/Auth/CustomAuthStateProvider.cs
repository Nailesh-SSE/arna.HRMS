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
    private const string EmployeeIdKey = "auth_employee_id";
    private const string AuthType = "jwt";

    private readonly ProtectedLocalStorage _protectedLocalStorage;
    private readonly ILogger<CustomAuthStateProvider> _logger;

    // In-memory cache to reduce interop calls
    private string? _cachedAccessToken;
    private string? _cachedRefreshToken;
    private int _cachedUserId;
    private int _cachedEmployeeId;
    private bool _isInitialized;

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

    public async Task LoginAsync(int userId, string accessToken, string refreshToken, int? employeeId = null)
    {
        try
        {
            var jwt = new JwtSecurityTokenHandler().ReadJwtToken(accessToken);

            await _protectedLocalStorage.SetAsync(AccessTokenKey, accessToken);
            await _protectedLocalStorage.SetAsync(RefreshTokenKey, refreshToken);
            await _protectedLocalStorage.SetAsync(UserIdKey, userId);

            if (employeeId.HasValue && employeeId.Value > 0)
            {
                await _protectedLocalStorage.SetAsync(EmployeeIdKey, employeeId.Value);
            }

            // Update cache
            _cachedAccessToken = accessToken;
            _cachedRefreshToken = refreshToken;
            _cachedUserId = userId;
            if (employeeId.HasValue && employeeId.Value > 0)
            {
                _cachedEmployeeId = employeeId.Value;
            }
            _isInitialized = true;

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

            // Update cache
            _cachedAccessToken = accessToken;
            _cachedRefreshToken = refreshToken;

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
            await _protectedLocalStorage.DeleteAsync(EmployeeIdKey);

            // Clear cache
            _cachedAccessToken = null;
            _cachedRefreshToken = null;
            _cachedUserId = 0;
            _cachedEmployeeId = 0;
            _isInitialized = false;

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
            // Return from cache if available and initialized
            if (_isInitialized && _cachedUserId > 0)
                return _cachedUserId;

            var result = await _protectedLocalStorage.GetAsync<int>(UserIdKey);
            if (result.Success && result.Value > 0)
            {
                _cachedUserId = result.Value;
                return result.Value;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user ID");
            return 0;
        }
    }

    public async Task<int> GetEmployeeIdAsync()
    {
        try
        {
            // Return from cache if available and initialized
            if (_isInitialized && _cachedEmployeeId > 0)
                return _cachedEmployeeId;

            var authState = await GetAuthenticationStateAsync();
            var user = authState.User;

            // Try to get employee ID from claims first
            if (user?.Identity?.IsAuthenticated == true)
            {
                var employeeIdClaim = user.FindFirst("employeeId") ?? user.FindFirst("EmployeeId") ?? user.FindFirst(ClaimTypes.NameIdentifier);
                if (employeeIdClaim != null && int.TryParse(employeeIdClaim.Value, out int employeeIdFromClaim) && employeeIdFromClaim > 0)
                {
                    _cachedEmployeeId = employeeIdFromClaim;
                    return employeeIdFromClaim;
                }
            }

            // Fall back to localStorage
            var result = await _protectedLocalStorage.GetAsync<int>(EmployeeIdKey);
            if (result.Success && result.Value > 0)
            {
                _cachedEmployeeId = result.Value;
                return result.Value;
            }

            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve employee ID");
            return 0;
        }
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        try
        {
            // Return from cache if available
            if (!string.IsNullOrWhiteSpace(_cachedAccessToken))
            {
                _logger.LogDebug("Returning access token from cache");
                return _cachedAccessToken;
            }

            // Retrieve from protected local storage
            var result = await _protectedLocalStorage.GetAsync<string>(AccessTokenKey);
            if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
            {
                _cachedAccessToken = result.Value;
                _isInitialized = true;
                _logger.LogDebug("Access token retrieved from storage and cached");
                return result.Value;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve access token");
            return null;
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        try
        {
            // Return from cache if available
            if (!string.IsNullOrWhiteSpace(_cachedRefreshToken))
            {
                _logger.LogDebug("Returning refresh token from cache");
                return _cachedRefreshToken;
            }

            // Retrieve from protected local storage
            var result = await _protectedLocalStorage.GetAsync<string>(RefreshTokenKey);
            if (result.Success && !string.IsNullOrWhiteSpace(result.Value))
            {
                _cachedRefreshToken = result.Value;
                _isInitialized = true;
                _logger.LogDebug("Refresh token retrieved from storage and cached");
                return result.Value;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve refresh token");
            return null;
        }
    }

    public async Task<(int userId, string? refreshToken)> GetRefreshDataAsync()
    {
        try
        {
            // Try to get from cache first
            if (_isInitialized && _cachedUserId > 0 && !string.IsNullOrWhiteSpace(_cachedRefreshToken))
            {
                _logger.LogDebug("Returning refresh data from cache");
                return (_cachedUserId, _cachedRefreshToken);
            }

            var userIdResult = await _protectedLocalStorage.GetAsync<int>(UserIdKey);
            var refreshTokenResult = await _protectedLocalStorage.GetAsync<string>(RefreshTokenKey);

            var userId = userIdResult.Success ? userIdResult.Value : 0;
            var refreshToken = refreshTokenResult.Success ? refreshTokenResult.Value : null;

            // Update cache
            if (userId > 0)
                _cachedUserId = userId;
            if (!string.IsNullOrWhiteSpace(refreshToken))
                _cachedRefreshToken = refreshToken;
            if (userId > 0 || !string.IsNullOrWhiteSpace(refreshToken))
                _isInitialized = true;

            return (userId, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve refresh data");
            return (0, null);
        }
    }

    /// <summary>
    /// Clears the in-memory token cache. Call this when you suspect the cache might be stale.
    /// </summary>
    public void ClearTokenCache()
    {
        _logger.LogDebug("Clearing token cache");
        _cachedAccessToken = null;
        _cachedRefreshToken = null;
        _isInitialized = false;
    }
}