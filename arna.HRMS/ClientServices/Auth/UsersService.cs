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
        var response = await HttpClient.GetFromJsonAsync<UserDto>($"api/users/{id}");
        return response;
    }

    public async Task<UserDto?> GetUserByNameAsync(string userName)
    {
        var response = await HttpClient.GetFromJsonAsync<UserDto>($"api/users/byname/{userName}");
        return response;
    }

    public async Task<UserDto> CreateUserAsync(UserDto userDto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/users", userDto);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UserDto>();
    }

    public async Task UpdateUserAsync(int id, UserDto userDto)
    {
        var updateRequest = new UserDto
        {
            Id = userDto.Id,
            Username = userDto.Username,
            Email = userDto.Email,
            Password = userDto.Password,
            FirstName = userDto.FirstName,
            LastName = userDto.LastName,
            Role = userDto.Role,
            IsActive = userDto.IsActive
        };
        var response = await HttpClient.PutAsJsonAsync($"api/users/{id}", updateRequest);
        response.EnsureSuccessStatusCode();
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var response = await HttpClient.DeleteAsync($"api/users/{id}");
        return response != null;
    }
}
