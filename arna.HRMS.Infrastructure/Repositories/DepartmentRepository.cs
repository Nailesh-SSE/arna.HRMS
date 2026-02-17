using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
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
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        var department = await _baseRepository.Query()
            .FirstOrDefaultAsync(d => d.Id == id && d.IsActive && !d.IsDeleted);

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
        var department = await GetDepartmentByIdAsync(id);

        if (department == null)
            return false;

        department.IsActive = false;
        department.IsDeleted = true;
        department.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(department);
        return true;
    }

    public async Task<bool> DepartmentExistsAsync(string name, int? id)
    {
        name = (name ?? string.Empty).Trim().ToLower();

        return await _baseRepository.Query()
            .AnyAsync(u =>
                u.IsActive &&
                !u.IsDeleted &&
                u.Name.Trim().ToLower() == name &&
                u.Id != id
            );
    }

}
