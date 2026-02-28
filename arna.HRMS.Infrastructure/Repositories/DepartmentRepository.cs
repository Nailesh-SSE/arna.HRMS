using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class DepartmentRepository
{
    private readonly IBaseRepository<Department> _baseRepository;

    public DepartmentRepository(IBaseRepository<Department> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<List<Department>> GetDepartmentsAsync()
    {
        return await _baseRepository.Query()
            .Where(d => d.IsActive && !d.IsDeleted)
            .Include(d => d.ParentDepartment)
            .OrderByDescending(d => d.Id) 
            .ToListAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .Where(d => d.Id == id && d.IsActive && !d.IsDeleted)
            .FirstOrDefaultAsync();
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
        var department = await _baseRepository.Query()
            .FirstOrDefaultAsync(d =>
                d.Id == id &&
                d.IsActive &&
                !d.IsDeleted);

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
        if (string.IsNullOrWhiteSpace(name))
            return false;

        name = name.Trim().ToLower();

        return await _baseRepository.Query()
            .Where(d => d.IsActive && !d.IsDeleted)
            .AnyAsync(d =>
                d.Id != id &&
                d.Name.Trim().ToLower() == name
            );
    }
}