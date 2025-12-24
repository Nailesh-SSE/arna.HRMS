using arna.HRMS.Models.DTOs;

namespace arna.HRMS.ClientServices.Employee;

public interface IDepartmentService
{
    Task<List<DepartmentDto>> GetDepartmentsAsync();
    Task<DepartmentDto?> GetDepartmentByIdAsync(int id);
    Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto DepartmentDto);
}
public class DepartmentService(HttpClient HttpClient) : IDepartmentService
{

    public async Task<List<DepartmentDto>> GetDepartmentsAsync()
    {
        var response = await HttpClient.GetFromJsonAsync<List<DepartmentDto>>("api/Department");
        return response ?? new List<DepartmentDto>();
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
    {
        var response = await HttpClient.GetFromJsonAsync<DepartmentDto>($"api/Department/{id}");
        return response;
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto DepartmentDto)
    {
        var response = await HttpClient.PostAsJsonAsync("api/Department", DepartmentDto);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<DepartmentDto>();
    }
}