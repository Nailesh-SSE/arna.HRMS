using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Auth;

public interface IUsersService
{
    Task<List<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserByIdAsync(int id);
    Task<UserDto?> GetUserByNameAsync(string userName);
    Task<UserDto> CreateUserAsync(UserDto userDto);
    Task UpdateUserAsync(int id, UserDto userDto);
    Task<bool> DeleteUserAsync(int id);
}

public class UsersService(HttpClient HttpClient) : IUsersService
{
    public async Task<List<UserDto>> GetUsersAsync()
    {
        var response = await HttpClient.GetAsync("api/users");
        if(response.IsSuccessStatusCode)
        {
            var users = await response.Content.ReadFromJsonAsync<List<UserDto>>();
            return users ?? new List<UserDto>();
        }
        return new List<UserDto>();
    }

    public async Task<UserDto?> GetUserByIdAsync(int id)
    {
        var response = await HttpClient.GetAsync($"api/users/{id}");
        if(response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            return user;
        }
        return new UserDto();
    }

    public async Task<UserDto?> GetUserByNameAsync(string userName)
    {
        var response = await HttpClient.GetAsync($"api/users/byname/{userName}");
        if(response.IsSuccessStatusCode)
        {
            var user = await response.Content.ReadFromJsonAsync<UserDto>();
            return user;
        }
        return new UserDto();
    }

    public async Task<UserDto> CreateUserAsync(UserDto userDto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/users", userDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task UpdateUserAsync(int id, UserDto userDto)
    {
        var response = await HttpClient.PutAsJsonAsync($"api/users/{id}", userDto);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var response = await HttpClient.DeleteAsync($"api/users/{id}");
        return response != null;
    }
}
