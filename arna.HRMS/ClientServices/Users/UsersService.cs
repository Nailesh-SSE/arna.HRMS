using arna.HRMS.ClientServices.Common;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Users;

public interface IUsersService
{
    Task<ApiResult<List<UserDto>>> GetUsersAsync();
    Task<ApiResult<UserDto>> GetUserByIdAsync(int id);
    Task<ApiResult<UserDto>> GetUserByNameAsync(string userName);
    Task<ApiResult<UserDto>> CreateUserAsync(UserDto userDto);
    Task<ApiResult<bool>> UpdateUserAsync(int id, UserDto userDto);
    Task<ApiResult<bool>> DeleteUserAsync(int id);
    Task<ApiResult<bool>> ChangeUserPasswordAsync(int id, string newPassword);
}

public class UsersService : IUsersService
{
    private readonly HttpService _http;

    public UsersService(HttpService http)
    {
        _http = http;
    }

    public async Task<ApiResult<List<UserDto>>> GetUsersAsync()
    {
        return await _http.GetAsync<List<UserDto>>("api/users");
    }

    public async Task<ApiResult<UserDto>> GetUserByIdAsync(int id)
    {
        return await _http.GetAsync<UserDto>($"api/users/{id}");
    }

    public async Task<ApiResult<UserDto>> GetUserByNameAsync(string userName)
    {
        return await _http.GetAsync<UserDto>($"api/users/byname/{userName}");
    }

    public async Task<ApiResult<UserDto>> CreateUserAsync(UserDto userDto)
    {
        return await _http.PostAsync<UserDto>("api/users", userDto);
    }

    public async Task<ApiResult<bool>> UpdateUserAsync(int id, UserDto userDto)
    {
        return await _http.PutAsync<bool>($"api/users/{id}", userDto);
    }

    public async Task<ApiResult<bool>> DeleteUserAsync(int id)
    {
        return await _http.DeleteAsync<bool>($"api/users/{id}");
    }

    public Task<ApiResult<bool>> ChangeUserPasswordAsync(int id, string newPassword)
    {
        return _http.PutAsync<bool>($"api/users/{id}/changepassword", newPassword);
    }
}
