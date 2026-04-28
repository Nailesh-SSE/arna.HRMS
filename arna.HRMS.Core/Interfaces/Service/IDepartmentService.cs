using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IDepartmentService
{
    Task<ServiceResult<List<DepartmentDto>>> GetDepartmentsAsync();
    Task<ServiceResult<DepartmentDto?>> GetDepartmentByIdAsync(int id);
    Task<ServiceResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto dto);
    Task<ServiceResult<DepartmentDto>> UpdateDepartmentAsync(DepartmentDto dto);
    Task<ServiceResult<bool>> DeleteDepartmentAsync(int id);
}