using arna.HRMS.Core.DTOs.Responses;
using Microsoft.AspNetCore.Identity.Data;

namespace arna.HRMS.ClientServices.Auth; 

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
}

public class AuthService(HttpClient HttpClient) : IAuthService
{
    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var response = await HttpClient.PostAsJsonAsync("api/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            return new AuthResponse
            {
                IsSuccess = false,
                Message = "Invalid email or password"
            };
        }

        return await response.Content.ReadFromJsonAsync<AuthResponse>()
               ?? new AuthResponse { IsSuccess = false };
    }
}
