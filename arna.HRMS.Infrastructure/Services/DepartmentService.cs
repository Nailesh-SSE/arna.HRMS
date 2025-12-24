using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly DepartmentRepository _departmentRepository;
    private readonly IMapper _mapper;

    public DepartmentService(DepartmentRepository departmentRepository, IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _mapper = mapper;
    }

    public async Task<List<DepartmentDto>> GetDepartmentAsync()
    {
        var Department = await _departmentRepository.GetDepartmentAsync();
        return Department.Select(e => _mapper.Map<DepartmentDto>(e)).ToList();
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
    {
        var depart = await _departmentRepository.GetDepartmentByIdAsync(id);
        if (depart == null) return null;
        //var employeedto = new EmployeeDto();
        var departmentdto = _mapper.Map<DepartmentDto>(depart);

        return departmentdto;
    }

    public async Task<DepartmentDto> CreateDepartmentAsync(Department department)
    {
        var createdDepartment= await _departmentRepository.CreateDepartmentAsync(department);
        return _mapper.Map<DepartmentDto>(createdDepartment);
    }
    public async Task<bool> DeleteDepartmentAsync(int id) 
    {
        var departmentDelete = await _departmentRepository.DeleteDepartmentAsync(id);
        return departmentDelete;
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(Department department)
    {
        var updatedDepartment = await _departmentRepository.UpdateDepartmentAsync(department);
        return _mapper.Map<DepartmentDto>(updatedDepartment);
    }
}
