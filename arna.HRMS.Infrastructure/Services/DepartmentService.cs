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
        return _mapper.Map<List<DepartmentDto>>(Department);
    }

    public async Task<DepartmentDto?> GetDepartmentByIdAsync(int id)
    {
        var depart = await _departmentRepository.GetDepartmentByIdAsync(id);
       return depart == null ? null : _mapper.Map<DepartmentDto>(depart);
    }
    
    public async Task<DepartmentDto> CreateDepartmentAsync(DepartmentDto departmentdto)
    {
        var department = _mapper.Map<Department>(departmentdto);
        var createdDepartment= await _departmentRepository.CreateDepartmentAsync(department);
        return _mapper.Map<DepartmentDto>(createdDepartment);
    }
    public async Task<bool> DeleteDepartmentAsync(int id) 
    {
        return await _departmentRepository.DeleteDepartmentAsync(id);
    }

    public async Task<DepartmentDto> UpdateDepartmentAsync(DepartmentDto departmentDto)
    {
        var department = _mapper.Map<Department>(departmentDto);
        var updatedDepartment = await _departmentRepository.UpdateDepartmentAsync(department);
        return _mapper.Map<DepartmentDto>(updatedDepartment);
    }
}
