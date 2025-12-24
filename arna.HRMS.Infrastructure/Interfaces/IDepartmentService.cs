using arna.HRMS.Core.Entities;
using arna.HRMS.Models.DTOs;
namespace arna.HRMS.Infrastructure.Interfaces;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetDepartmentAsync();
    Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
    Task<DepartmentDto> CreateDepartmentAsync(Department department);
    Task<bool> DeleteDepartmentAsync(int id);
    Task<DepartmentDto> UpdateDepartmentAsync(Department department);
}