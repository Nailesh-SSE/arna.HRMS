// ============================================================
// FILE: arna.HRMS/Services/Auth/AuthService.cs
// ============================================================
using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services.Auth;

// ──────────────────────────────────────────────────────────────
// HOW THIS WORKS:
//
//  This is the Web app's auth service (NOT the API's AuthService).
//  It calls the API via ApiClients, then tells CustomAuthStateProvider
//  to store the tokens and update the Blazor auth state.
//
//  LOGIN FLOW:
//    Login.razor → AuthService.LoginAsync()
//      → POST api/auth/login (via ApiClients)
//      → On success: CustomAuthStateProvider.LoginAsync()
//          → saves tokens to ProtectedSessionStorage
//          → NotifyAuthenticationStateChanged()
//          → all components (NavMenu, Header etc.) re-render
//      → Navigate to "/"
//
//  LOGOUT FLOW:
//    UserMenu.razor → AuthService.LogoutAsync()
//      → POST api/auth/logout (revokes refresh token in DB)
//      → CustomAuthStateProvider.LogoutAsync()
//          → clears ProtectedSessionStorage
//          → NotifyAuthenticationStateChanged(Anonymous)
//          → redirect to /login
// ──────────────────────────────────────────────────────────────

public interface IAuthService
{
    Task<bool> LoginAsync(LoginViewModel request);
    Task LogoutAsync();
}

public sealed class AuthService : IAuthService
{
    private readonly ApiClients.AuthApi _authApi;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(ApiClients api, CustomAuthStateProvider authStateProvider)
    {
        _authApi = api.Auth;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(LoginViewModel request)
    {
        // Step 1: Call API login endpoint
        var result = await _authApi.LoginAsync(request);

        if (!result.IsSuccess || result.Data is null)
            return false;

        if (string.IsNullOrWhiteSpace(result.Data.AccessToken) ||
            string.IsNullOrWhiteSpace(result.Data.RefreshToken))
            return false;

        // Step 2: Store tokens + update Blazor auth state
        // This triggers NotifyAuthenticationStateChanged internally
        await _authStateProvider.LoginAsync(
            result.Data.UserId,
            result.Data.AccessToken,
            result.Data.RefreshToken);

        return true;
    }

    public async Task LogoutAsync()
    {
        // Step 1: Tell API to revoke refresh token in DB
        // Wrapped in try/catch — we always want to clear local state
        try
        {
            await _authApi.LogoutAsync();
        }
        catch
        {
            // Ignore API errors — proceed with local logout
        }

        // Step 2: Clear tokens from storage + set Anonymous state
        await _authStateProvider.LogoutAsync();
    }
}