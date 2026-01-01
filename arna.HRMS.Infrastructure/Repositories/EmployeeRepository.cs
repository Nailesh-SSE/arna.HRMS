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
        return await _baseRepository.Query().Where(x => x.IsActive && !x.IsDeleted).Include(e => e.Department).ToListAsync();
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

    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var employee =  await _baseRepository.GetByIdAsync(id);

        if (employee == null)
            return false;

        employee.IsActive = false;
        employee.IsDeleted = true;
        employee.UpdatedAt = DateTime.UtcNow;

        await _baseRepository.UpdateAsync(employee);
        return true;
    }

    public async Task<bool> EmployeeExistsAsync(string email, string phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(phoneNumber))
            return false;
        email = email?.Trim().ToLower() ?? string.Empty;
        phoneNumber = phoneNumber?.Trim() ?? string.Empty;
        return await _baseRepository.Query().AnyAsync(e =>
            e.Email.ToLower() == email ||
            e.PhoneNumber == phoneNumber);
    }
}
