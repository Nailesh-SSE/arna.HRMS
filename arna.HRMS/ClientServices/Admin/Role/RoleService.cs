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
        _roleApi = apiClients.Roles;
    }

    public Task<ApiResult<List<RoleViewModel>>> GetRolesAsync()
        => _roleApi.GetAll();

    public Task<ApiResult<RoleViewModel>> GetRoleByIdAsync(int id)
        => _roleApi.GetById(id);

    public Task<ApiResult<RoleViewModel>> CreateRoleAsync(RoleViewModel model)
        => _roleApi.Create(model);

    public Task<ApiResult<bool>> UpdateRoleAsync(int id, RoleViewModel model)
        => _roleApi.Update(id, model);

    public Task<ApiResult<bool>> DeleteRoleAsync(int id)
        => _roleApi.Delete(id);
}
