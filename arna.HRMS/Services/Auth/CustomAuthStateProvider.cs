using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;

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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "JS interop unavailable during prerendering — LoginAsync skipped storage write");
            _cachedAccessToken = accessToken;
            _cachedRefreshToken = refreshToken;
            _cachedUserId = userId;
            if (employeeId.HasValue && employeeId.Value > 0)
                _cachedEmployeeId = employeeId.Value;
            _isInitialized = true;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "Browser storage write cancelled during login; using in-memory auth cache");
            _cachedAccessToken = accessToken;
            _cachedRefreshToken = refreshToken;
            _cachedUserId = userId;
            if (employeeId.HasValue && employeeId.Value > 0)
                _cachedEmployeeId = employeeId.Value;
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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "JS interop unavailable — UpdateTokensAsync updating cache only");
            _cachedAccessToken = accessToken;
            _cachedRefreshToken = refreshToken;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "Browser storage write cancelled during token update; using in-memory auth cache");
            _cachedAccessToken = accessToken;
            _cachedRefreshToken = refreshToken;
            _isInitialized = true;
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
            try
            {
                await _protectedLocalStorage.DeleteAsync(AccessTokenKey);
                await _protectedLocalStorage.DeleteAsync(RefreshTokenKey);
                await _protectedLocalStorage.DeleteAsync(UserIdKey);
                await _protectedLocalStorage.DeleteAsync(EmployeeIdKey);
            }
            catch (InvalidOperationException ex)
            {
                // ✅ FIX: JS interop unavailable during prerendering — clear cache only
                _logger.LogWarning(ex, "JS interop unavailable — LogoutAsync clearing cache only");
            }

            catch (OperationCanceledException ex)
            {
                _logger.LogDebug(ex, "Browser storage delete cancelled during logout; clearing in-memory auth cache");
            }

            // Always clear cache regardless of storage outcome
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
        if (_isInitialized && _cachedUserId > 0)
            return _cachedUserId;

        var val = await SafeGetStorageValueAsync<int>(UserIdKey, _cachedUserId);
        if (val > 0)
        {
            _cachedUserId = val;
            _isInitialized = true;
            return val;
        }

        return 0;
    }

    public async Task<int> GetEmployeeIdAsync()
    {
        if (_isInitialized && _cachedEmployeeId > 0)
            return _cachedEmployeeId;

        var authState = await GetAuthenticationStateAsync();
        var user = authState.User;

        if (user?.Identity?.IsAuthenticated == true)
        {
            var employeeIdClaim = user.FindFirst("employeeId") ?? user.FindFirst("EmployeeId") ?? user.FindFirst(ClaimTypes.NameIdentifier);
            if (employeeIdClaim != null && int.TryParse(employeeIdClaim.Value, out int employeeIdFromClaim) && employeeIdFromClaim > 0)
            {
                _cachedEmployeeId = employeeIdFromClaim;
                return employeeIdFromClaim;
            }
        }

        var val = await SafeGetStorageValueAsync<int>(EmployeeIdKey, _cachedEmployeeId);
        if (val > 0)
        {
            _cachedEmployeeId = val;
            return val;
        }

        return 0;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedAccessToken))
            return _cachedAccessToken;

        var token = await SafeGetStorageValueAsync<string>(AccessTokenKey, _cachedAccessToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            _cachedAccessToken = token;
            _isInitialized = true;
            return token;
        }

        return null;
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        if (!string.IsNullOrWhiteSpace(_cachedRefreshToken))
            return _cachedRefreshToken;

        var token = await SafeGetStorageValueAsync<string>(RefreshTokenKey, _cachedRefreshToken);
        if (!string.IsNullOrWhiteSpace(token))
        {
            _cachedRefreshToken = token;
            return token;
        }

        return null;
    }

    public async Task<(int userId, string? refreshToken)> GetRefreshDataAsync()
    {
        if (_isInitialized && _cachedUserId > 0 && !string.IsNullOrWhiteSpace(_cachedRefreshToken))
            return (_cachedUserId, _cachedRefreshToken);

        var userId = await GetUserIdAsync();
        var refreshToken = await GetRefreshTokenAsync();

        return (userId, refreshToken);
    }

    private async Task<T?> SafeGetStorageValueAsync<T>(string key, T? cachedValue)
    {
        try
        {
            var result = await _protectedLocalStorage.GetAsync<T>(key);
            if (result.Success && result.Value != null)
            {
                return result.Value;
            }
            return default;
        }
        catch (InvalidOperationException)
        {
            return cachedValue;
        }
        catch (OperationCanceledException)
        {
            return cachedValue;
        }
        catch (CryptographicException)
        {
            await ClearStoredAuthDataAsync();
            return default;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve {Key} from protected local storage", key);
            return default;
        }
    }

    private async Task ClearStoredAuthDataAsync()
    {
        _cachedAccessToken = null;
        _cachedRefreshToken = null;
        _cachedUserId = 0;
        _cachedEmployeeId = 0;
        _isInitialized = false;

        try
        {
            await _protectedLocalStorage.DeleteAsync(AccessTokenKey);
            await _protectedLocalStorage.DeleteAsync(RefreshTokenKey);
            await _protectedLocalStorage.DeleteAsync(UserIdKey);
            await _protectedLocalStorage.DeleteAsync(EmployeeIdKey);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogDebug(ex, "JS interop unavailable while clearing stale authentication data");
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogDebug(ex, "Browser storage delete cancelled while clearing stale authentication data");
        }
        catch (CryptographicException ex)
        {
            _logger.LogDebug(ex, "Stored authentication data was already unreadable while clearing it");
        }

        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
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
