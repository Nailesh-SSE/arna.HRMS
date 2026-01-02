using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class DepartmentRepository
{
    private readonly IBaseRepository<Department> _baseRepository;

    public DepartmentRepository(IBaseRepository<Department> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<IEnumerable<Department>> GetDepartmentAsync()
    {
        return await _baseRepository.Query()
            .Include(d => d.ParentDepartment)
            .Where(d => d.IsActive && !d.IsDeleted)
            .ToListAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        var department = await _baseRepository.GetByIdAsync(id);
        return department;
    }

    public Task<Department> CreateDepartmentAsync(Department department)
    {
        return _baseRepository.AddAsync(department);
    }

    public Task<Department> UpdateDepartmentAsync(Department department)
    {
        return _baseRepository.UpdateAsync(department);
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        var department = await _baseRepository.GetByIdAsync(id);

        if (department == null)
            return false;

        department.IsActive = false;
        department.IsDeleted = true;
        department.UpdatedAt = DateTime.UtcNow;

        await _baseRepository.UpdateAsync(department);
        return true;
    }
}
