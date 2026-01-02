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

    public Task<ApiResult<List<UserDto>>> GetUsersAsync()
    {
        return _http.GetAsync<List<UserDto>>("api/users");
    }

    public Task<ApiResult<UserDto>> GetUserByIdAsync(int id)
    {
        return _http.GetAsync<UserDto>($"api/users/{id}");
    }

    public Task<ApiResult<UserDto>> GetUserByNameAsync(string userName)
    {
        return _http.GetAsync<UserDto>($"api/users/byname/{userName}");
    }

    public Task<ApiResult<UserDto>> CreateUserAsync(UserDto userDto)
    {
        return _http.PostAsync<UserDto>("api/users", userDto);
    }

    public Task<ApiResult<bool>> UpdateUserAsync(int id, UserDto userDto)
    {
        return _http.PutAsync<bool>($"api/users/{id}", userDto);
    }

    public Task<ApiResult<bool>> DeleteUserAsync(int id)
    {
        return _http.DeleteAsync<bool>($"api/users/{id}");
    }

    public Task<ApiResult<bool>> ChangeUserPasswordAsync(int id, string newPassword)
    {
        return _http.PutAsync<bool>($"api/users/{id}/changepassword", newPassword);
    }
}
