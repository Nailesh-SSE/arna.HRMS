using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services.Auth;

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
        var result = await _authApi.LoginAsync(request);

        if (!result.IsSuccess || result.Data is null)
            return false;

        if (string.IsNullOrWhiteSpace(result.Data.AccessToken) || string.IsNullOrWhiteSpace(result.Data.RefreshToken))
            return false;

        await _authStateProvider.LoginAsync(result.Data.UserId, result.Data.AccessToken, result.Data.RefreshToken);

        return true;
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _authApi.LogoutAsync();
        }
        catch
        {
            // Ignore backend errors intentionally
        }

        await _authStateProvider.LogoutAsync();
    }
}