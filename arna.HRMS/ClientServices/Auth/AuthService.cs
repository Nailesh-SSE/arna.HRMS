using arna.HRMS.Core.DTOs.Responses;
using Microsoft.AspNetCore.Identity.Data;

namespace arna.HRMS.ClientServices.Auth; 

public interface IAuthService
{
    Task<bool> LoginAsync(LoginRequest request);
    Task LogoutAsync();
}

public class AuthService(HttpClient HttpClient, CustomAuthStateProvider CustomAuthStateProvider) : IAuthService
{
    public async Task<bool> LoginAsync(LoginRequest request)
    {
        var response = await HttpClient.PostAsJsonAsync("api/auth/login", request);
        if (!response.IsSuccessStatusCode) return false;

        var auth = await response.Content.ReadFromJsonAsync<AuthResponse>();
        await CustomAuthStateProvider.SetTokenAsync(auth!.AccessToken);

        return true;
    }

    public async Task LogoutAsync()
    {
        await HttpClient.PostAsync("api/auth/logout", null);
        await CustomAuthStateProvider.ClearTokenAsync();
    }
}
