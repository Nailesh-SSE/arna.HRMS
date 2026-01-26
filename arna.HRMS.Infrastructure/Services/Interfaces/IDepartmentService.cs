using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
namespace arna.HRMS.Infrastructure.Services.Interfaces;

public interface IDepartmentService
{
    Task<ServiceResult<List<DepartmentDto>>> GetDepartmentAsync();
    Task<ServiceResult<DepartmentDto?>> GetDepartmentByIdAsync(int id);
    Task<ServiceResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto departmentDto);
    Task<ServiceResult<DepartmentDto>> UpdateDepartmentAsync(DepartmentDto departmentDto);
    Task<ServiceResult<bool>> DeleteDepartmentAsync(int id);
}