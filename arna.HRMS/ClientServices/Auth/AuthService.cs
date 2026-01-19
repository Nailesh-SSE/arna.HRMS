using arna.HRMS.ClientServices.Http;
using Microsoft.AspNetCore.Identity.Data;

namespace arna.HRMS.ClientServices.Auth;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginRequest request);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private readonly ApiClients.AuthApi _authApi;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(ApiClients api, CustomAuthStateProvider authStateProvider)
    {
        _authApi = api.Auth;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        var result = await _authApi.Login(request);

        if (!result.IsSuccess || result.Data is null)
            return false;

        if (string.IsNullOrWhiteSpace(result.Data.AccessToken))
            return false;

        if (string.IsNullOrWhiteSpace(result.Data.RefreshToken))
            return false;

        await _authStateProvider.LoginAsync(
            result.Data.UserId,
            result.Data.AccessToken,
            result.Data.RefreshToken
        );

        return true;
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _authApi.Logout();
        }
        catch
        {
            // ignore backend errors
        }

        await _authStateProvider.LogoutAsync();
    }
}
