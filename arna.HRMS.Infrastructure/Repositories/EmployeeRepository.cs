using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class EmployeeRepository
{
    private readonly IBaseRepository<Employee> _baseRepository;

    public EmployeeRepository(IBaseRepository<Employee> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<List<Employee>> GetEmployeesAsync()
    {
        return await _baseRepository.Query()
            .Where(e => e.IsActive && !e.IsDeleted)
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .OrderByDescending(e => e.Id)
            .ToListAsync();
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .Where(e => e.Id == id && e.IsActive && !e.IsDeleted)
            .Include(e => e.Department)
            .Include(e => e.Manager)
            .FirstOrDefaultAsync();
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
        var employee = await _baseRepository.Query()
            .FirstOrDefaultAsync(e =>
                e.Id == id &&
                e.IsActive &&
                !e.IsDeleted);

        if (employee == null)
            return false;

        employee.IsActive = false;
        employee.IsDeleted = true;
        employee.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(employee);
        return true;
    }

    public async Task<bool> EmployeeExistsAsync(string? email, string? phoneNumber, int? employeeId = 0)
    {
        if (string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        email = email?.Trim().ToLower();
        phoneNumber = phoneNumber?.Trim().ToLower();

        return await _baseRepository.Query()
            .Where(e => e.IsActive && !e.IsDeleted)
            .AnyAsync(e =>
               e.Id != employeeId &&
                (
                    (!string.IsNullOrEmpty(email) && e.Email.Trim().ToLower() == email) ||
                    (!string.IsNullOrEmpty(phoneNumber) && e.PhoneNumber.Trim().ToLower() == phoneNumber)
                )
            );
    }

    public async Task<string?> GetLastEmployeeNumberAsync()
    {
        return await _baseRepository.Query()
            .Where(e => e.EmployeeNumber != null)
            .OrderByDescending(e => e.EmployeeNumber)
            .Select(e => e.EmployeeNumber)
            .FirstOrDefaultAsync();
    }
}