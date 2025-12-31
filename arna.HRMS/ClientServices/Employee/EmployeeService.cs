using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Employee;

public interface IEmployeeService
{
    Task<List<EmployeeDto>> GetEmployeesAsync();
    Task<EmployeeDto?> GetEmployeeByIdAsync(int id);
    Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto);
    Task<bool> DeleteEmployeeAsync(int id);
    Task UpdateEmployeeAsync(int id, EmployeeDto employeeDto);
}
public class EmployeeService(HttpClient HttpClient) : IEmployeeService
{
 
    public async Task<List<EmployeeDto>> GetEmployeesAsync()
    {
        var response = await HttpClient.GetFromJsonAsync<List<EmployeeDto>>("api/employees");
        return response ?? new List<EmployeeDto>();
    }

    public async Task<EmployeeDto?> GetEmployeeByIdAsync(int id)
    {
        var response = await HttpClient.GetFromJsonAsync<EmployeeDto>($"api/employees/{id}");
        return response;
    }

    public async Task<EmployeeDto> CreateEmployeeAsync(EmployeeDto employeeDto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/employees", employeeDto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EmployeeDto>();
    }
    public async Task<bool> DeleteEmployeeAsync(int id)
    {
        var respnce = await HttpClient.DeleteAsync($"api/employees/{id}");
        return respnce != null;
    }
    public async Task UpdateEmployeeAsync(int id, EmployeeDto employeeDto)
    {
        var updateRequest = new EmployeeDto
        {
            Id= employeeDto.Id,
            EmployeeNumber = employeeDto.EmployeeNumber,
            FirstName = employeeDto.FirstName,
            LastName = employeeDto.LastName,
            Email = employeeDto.Email,
            PhoneNumber = employeeDto.PhoneNumber,
            Position = employeeDto.Position,
            Salary = employeeDto.Salary,
            DateOfBirth = employeeDto.DateOfBirth,
            HireDate = employeeDto.HireDate,
            DepartmentId = employeeDto.DepartmentId,
            ManagerId = employeeDto.ManagerId
        };
        var response = await HttpClient.PutAsJsonAsync($"api/employees/{id}", updateRequest);

        response.EnsureSuccessStatusCode();
    }
}
