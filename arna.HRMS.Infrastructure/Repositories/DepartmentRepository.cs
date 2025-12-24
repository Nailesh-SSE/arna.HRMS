using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;

namespace arna.HRMS.Infrastructure.Repositories;

public class DepartmentRepository
{
    private readonly IBaseRepository<Department> _baseRepository;

    public DepartmentRepository(IBaseRepository<Department> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public Task<IEnumerable<Department>> GetDepartmentAsync()
    {
        return _baseRepository.GetAllAsync();
    }

    public async Task<Department?> GetDepartmentByIdAsync(int id)
    {
        var department = await _baseRepository.GetByIdAsync(id).ConfigureAwait(false);
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

    public Task<bool> DeleteDepartmentAsync(int id)
    {
        return _baseRepository.DeleteAsync(id);
    }
}
