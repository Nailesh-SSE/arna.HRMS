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

    public Task<ApiResult<List<UserViewModel>>> GetUsersAsync() =>
        _userApi.GetAll();

    public Task<ApiResult<UserViewModel>> GetUserByIdAsync(int id) =>
        _userApi.GetById(id);

    public Task<ApiResult<UserViewModel>> CreateUserAsync(UserViewModel model) =>
        _userApi.Create(model);

    public Task<ApiResult<bool>> UpdateUserAsync(int id, UserViewModel model) =>
        _userApi.Update(id, model);

    public Task<ApiResult<bool>> DeleteUserAsync(int id) =>
        _userApi.Delete(id);

    public Task<ApiResult<bool>> ChangeUserPasswordAsync(int id, string newPassword) =>
        _userApi.ChangePassword(id, newPassword);
}
