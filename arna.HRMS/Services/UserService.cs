using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services;

public interface IUserService
{
    Task<ApiResult<List<UserViewModel>>> GetUsersAsync();
    Task<ApiResult<UserViewModel>> GetUserByIdAsync(int id);
    Task<ApiResult<UserViewModel>> CreateUserAsync(UserViewModel model);
    Task<ApiResult<bool>> UpdateUserAsync(int id, UserViewModel model);
    Task<ApiResult<bool>> DeleteUserAsync(int id);
    Task<ApiResult<bool>> ChangeUserPasswordAsync(int id, string newPassword);
}

public class UserService : IUserService
{
    private readonly ApiClients.UserApi _userApi;

    public UserService(ApiClients api) 
    {
        _userApi = api.Users; 
    }

    public async Task<ApiResult<List<UserViewModel>>> GetUsersAsync()
    {
        return await _userApi.GetAllAsync();
    }

    public async Task<ApiResult<UserViewModel>> GetUserByIdAsync(int id)
    {
        return await _userApi.GetByIdAsync(id);
    }

    public async Task<ApiResult<UserViewModel>> CreateUserAsync(UserViewModel model)
    {
        return await _userApi.CreateAsync(model);
    }

    public async Task<ApiResult<bool>> UpdateUserAsync(int id, UserViewModel model)
    {
        return await _userApi.UpdateAsync(id, model);
    }

    public async Task<ApiResult<bool>> DeleteUserAsync(int id)
    {
        return await _userApi.DeleteAsync(id);
    }

    public async Task<ApiResult<bool>> ChangeUserPasswordAsync(int id, string newPassword)
    {
        return await _userApi.ChangePasswordAsync(id, newPassword);
    }
}