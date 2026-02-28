using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services;

public interface IRoleService
{
    Task<ApiResult<List<RoleViewModel>>> GetRolesAsync();
    Task<ApiResult<RoleViewModel>> GetRoleByIdAsync(int id);
    Task<ApiResult<RoleViewModel>> CreateRoleAsync(RoleViewModel model);
    Task<ApiResult<bool>> UpdateRoleAsync(int id, RoleViewModel model);
    Task<ApiResult<bool>> DeleteRoleAsync(int id);
}

public class RoleService : IRoleService
{
    private readonly ApiClients.RoleApi _roleApi;

    public RoleService(ApiClients apiClients)
    {
        _roleApi = apiClients.Roles;
    }

    public async Task<ApiResult<List<RoleViewModel>>> GetRolesAsync()
    {
        return await _roleApi.GetAllAsync();
    }

    public async Task<ApiResult<RoleViewModel>> GetRoleByIdAsync(int id)
    {
        return await _roleApi.GetByIdAsync(id);
    }

    public async Task<ApiResult<RoleViewModel>> CreateRoleAsync(RoleViewModel model)
    {
        return await _roleApi.CreateAsync(model);
    }

    public async Task<ApiResult<bool>> UpdateRoleAsync(int id, RoleViewModel model)
    {
        return await _roleApi.UpdateAsync(id, model);
    }

    public async Task<ApiResult<bool>> DeleteRoleAsync(int id)
    {
        return await _roleApi.DeleteAsync(id);
    }
}