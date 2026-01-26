using arna.HRMS.ClientServices.Http;
using arna.HRMS.Models.Common;
using arna.HRMS.Models.ViewModels;

namespace arna.HRMS.ClientServices.Admin.Role;

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
        _roleApi = apiClients.Role;
    }

    public async Task<ApiResult<List<RoleViewModel>>> GetRolesAsync()
        => await _roleApi.GetAll();

    public async Task<ApiResult<RoleViewModel>> GetRoleByIdAsync(int id)
        => await _roleApi.GetById(id);

    public async Task<ApiResult<RoleViewModel>> CreateRoleAsync(RoleViewModel model)
        => await _roleApi.Create(model);

    public async Task<ApiResult<bool>> UpdateRoleAsync(int id, RoleViewModel model)
        => await _roleApi.Update(id, model);

    public async Task<ApiResult<bool>> DeleteRoleAsync(int id)
        => await _roleApi.Delete(id);
}
