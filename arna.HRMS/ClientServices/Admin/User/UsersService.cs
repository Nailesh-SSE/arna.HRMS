using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels;

namespace arna.HRMS.ClientServices.Admin.User;

public interface IUsersService
{
    Task<ApiResult<List<UserViewModel>>> GetUsersAsync();
    Task<ApiResult<UserViewModel>> GetUserByIdAsync(int id);
    Task<ApiResult<UserViewModel>> CreateUserAsync(UserViewModel model);
    Task<ApiResult<bool>> UpdateUserAsync(int id, UserViewModel model);
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

    public async Task<ApiResult<List<UserViewModel>>> GetUsersAsync()
        => await _userApi.GetAll();

    public async Task<ApiResult<UserViewModel>> GetUserByIdAsync(int id)
        => await _userApi.GetById(id);

    public async Task<ApiResult<UserViewModel>> CreateUserAsync(UserViewModel model)
        => await _userApi.Create(model);

    public async Task<ApiResult<bool>> UpdateUserAsync(int id, UserViewModel model)
        => await _userApi.Update(id, model);

    public async Task<ApiResult<bool>> DeleteUserAsync(int id)
        => await _userApi.Delete(id);

    public async Task<ApiResult<bool>> ChangeUserPasswordAsync(int id, string newPassword)
        => await _userApi.ChangePassword(id, newPassword);
}
