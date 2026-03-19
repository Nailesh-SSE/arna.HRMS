using arna.HRMS.Models.ViewModels.Auth;
using arna.HRMS.Services.Http;
using Microsoft.Extensions.Logging;

namespace arna.HRMS.Services.Auth;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginViewModel request);
    Task<bool> LogoutAsync(int userId);
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

            await _authStateProvider.LoginAsync(
                result.Data.UserId,
                result.Data.AccessToken,
                result.Data.RefreshToken);

            return true;
        }

    public async Task<bool> LogoutAsync(int userId)
    {
        try
        {

            if (userId <= 0)
            {
                return false;
            }

            try
            {
                var result = await _authApi.LogoutAsync(userId);
                
            }
            catch (Exception ex)
            {            }

            await _authStateProvider.LogoutAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            await _authStateProvider.LogoutAsync();
            return true;
        }
    }
}