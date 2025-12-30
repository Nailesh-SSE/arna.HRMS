using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class EmployeeRepository
{
    private readonly IBaseRepository<Employee> _baseRepository;

    public EmployeeRepository(IBaseRepository<Employee> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<IEnumerable<Employee>> GetEmployeesAsync()
    {
        return await _baseRepository.Query().Include(e => e.Department).ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        var employee = await _baseRepository.GetByIdAsync(id).ConfigureAwait(false);
        //return new Employee();
        return employee;
    }

    public Task<Employee> CreateEmployeeAsync(Employee employee)
    {
        return _baseRepository.AddAsync(employee);
    }

    public Task<Employee> UpdateEmployeeAsync(Employee employee)
    {
        return _baseRepository.UpdateAsync(employee);
    }

    public Task<bool> DeleteEmployeeAsync(int id)
    {
        return _baseRepository.DeleteAsync(id);
    }
}
