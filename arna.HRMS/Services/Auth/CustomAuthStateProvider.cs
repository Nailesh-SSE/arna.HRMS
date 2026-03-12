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

        await _sessionStorage.SetAsync(AccessTokenKey, accessToken);
        await _sessionStorage.SetAsync(RefreshTokenKey, refreshToken);
        await _sessionStorage.SetAsync(UserIdKey, userId);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task LogoutAsync()
    {
        await _sessionStorage.DeleteAsync(AccessTokenKey);
        await _sessionStorage.DeleteAsync(RefreshTokenKey);
        await _sessionStorage.DeleteAsync(UserIdKey);

        NotifyAuthenticationStateChanged(Task.FromResult(Anonymous));
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var result = await _sessionStorage.GetAsync<string>(AccessTokenKey);
        return result.Success ? result.Value : null;
    }

    public async Task<(int userId, string? refreshToken)> GetRefreshDataAsync()
    {
        var userIdResult = await _sessionStorage.GetAsync<int>(UserIdKey);
        var refreshTokenResult = await _sessionStorage.GetAsync<string>(RefreshTokenKey);
        int userId = userIdResult.Success ? userIdResult.Value : 0;
        string? refreshToken = refreshTokenResult.Success ? refreshTokenResult.Value : null;
        return (userId, refreshToken);
    }

    public async Task UpdateTokensAsync(string newAccessToken, string newRefreshToken)
    {
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(newAccessToken);
        var expiry = jwt.ValidTo;

        await _sessionStorage.SetAsync(AccessTokenKey, newAccessToken);
        await _sessionStorage.SetAsync(RefreshTokenKey, newRefreshToken);

        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }
}