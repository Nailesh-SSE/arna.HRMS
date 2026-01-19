using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Admin.User;

public interface IUsersService
{
    Task<ApiResult<List<UserDto>>> GetUsersAsync();
    Task<ApiResult<UserDto>> GetUserByIdAsync(int id);
    Task<ApiResult<UserDto>> CreateUserAsync(UserDto userDto);
    Task<ApiResult<bool>> UpdateUserAsync(int id, UserDto userDto);
    Task<ApiResult<bool>> DeleteUserAsync(int id);
    Task<ApiResult<bool>> ChangeUserPasswordAsync(int id, string newPassword);
}

public class UsersService : IUsersService
{
    private readonly ApiClients.UserApi _userApi;

    public UsersService(ApiClients api)
    {
        _userApi = api.Users;
    }

    public async Task<ApiResult<List<UserDto>>> GetUsersAsync()
        => await _userApi.GetAll();

    public async Task<ApiResult<UserDto>> GetUserByIdAsync(int id)
        => await _userApi.GetById(id);

    public async Task<ApiResult<UserDto>> CreateUserAsync(UserDto userDto)
        => await _userApi.Create(userDto);

    public async Task<ApiResult<bool>> UpdateUserAsync(int id, UserDto userDto)
        => await _userApi.Update(id, userDto);

    public async Task<ApiResult<bool>> DeleteUserAsync(int id)
        => await _userApi.Delete(id);

    public async Task<ApiResult<bool>> ChangeUserPasswordAsync(int id, string newPassword)
        => await _userApi.ChangePassword(id, newPassword);
}
