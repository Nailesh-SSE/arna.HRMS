using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IRoleService
{
    Task<ServiceResult<List<RoleDto>>> GetRolesAsync();
    Task<ServiceResult<RoleDto?>> GetRoleByIdAsync(int id);
    Task<ServiceResult<RoleDto?>> GetRoleByNameAsync(string name);
    Task<ServiceResult<RoleDto>> CreateRoleAsync(RoleDto dto);
    Task<ServiceResult<RoleDto>> UpdateRoleAsync(RoleDto dto);
    Task<ServiceResult<bool>> DeleteRoleAsync(int id);
}