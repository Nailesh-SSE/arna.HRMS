using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;

namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface IRoleService
{
    Task<ServiceResult<List<RoleDto>>> GetRoleAsync();
    Task<ServiceResult<RoleDto?>> GetRoleByIdAsync(int id);
    Task<ServiceResult<RoleDto?>> GetRoleByNameAsync(string name);  
    Task<ServiceResult<RoleDto>> CreateRoleAsync(RoleDto dto);
    Task<ServiceResult<RoleDto>> UpdateRoleAsync(RoleDto dto);
    Task<ServiceResult<bool>> DeleteRoleAsync(int id);
}
