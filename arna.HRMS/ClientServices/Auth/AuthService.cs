using arna.HRMS.ClientServices.Common;
using arna.HRMS.Core.DTOs.Responses;
using Microsoft.AspNetCore.Identity.Data;

namespace arna.HRMS.ClientServices.Auth;

public interface IAuthService
{
    Task<bool> LoginAsync(LoginRequest request);
    Task LogoutAsync();
}

public class AuthService : IAuthService
{
    private readonly HttpService _httpService;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthService(
        HttpService httpService,
        CustomAuthStateProvider authStateProvider)
    {
        _httpService = httpService;
        _authStateProvider = authStateProvider;
    }

    public async Task<bool> LoginAsync(LoginRequest request)
    {
        var result = await _httpService.PostAsync<AuthResponse>(
            "api/auth/login", request);

        if (!result.IsSuccess || result.Data is null)
            return false;

        if (string.IsNullOrWhiteSpace(result.Data.AccessToken))
            return false;

        await _authStateProvider.LoginAsync(result.Data.AccessToken);

        return true;
    }

    public async Task LogoutAsync()
    {
        try
        {
            await _httpService.PostAsync<bool>("api/auth/logout", new { });
        }
        catch
        {
            // Ignore backend errors
        }

        await _authStateProvider.LogoutAsync();
    }
}
