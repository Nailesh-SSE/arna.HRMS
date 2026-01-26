using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
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

    public async Task<ServiceResult<List<DepartmentDto>>> GetDepartmentAsync()
    {
        var departments = await _departmentRepository.GetDepartmentAsync();
        var list = _mapper.Map<List<DepartmentDto>>(departments);

        return ServiceResult<List<DepartmentDto>>.Success(list);
    }

    public async Task<ServiceResult<DepartmentDto?>> GetDepartmentByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<DepartmentDto?>.Fail("Invalid Department ID");

        var department = await _departmentRepository.GetDepartmentByIdAsync(id);

        if (department == null)
            return ServiceResult<DepartmentDto?>.Fail("Department not found");

        var dto = _mapper.Map<DepartmentDto>(department);
        return ServiceResult<DepartmentDto?>.Success(dto);
    }

    public async Task<ServiceResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto departmentDto)
    {
        if (departmentDto == null)
            return ServiceResult<DepartmentDto>.Fail("Invalid request");

        if (string.IsNullOrWhiteSpace(departmentDto.Name))
            return ServiceResult<DepartmentDto>.Fail("Department Name is required");

        var department = _mapper.Map<Department>(departmentDto);
        var created = await _departmentRepository.CreateDepartmentAsync(department);
        var resultDto = _mapper.Map<DepartmentDto>(created);

        return ServiceResult<DepartmentDto>.Success(resultDto, "Department created successfully");
    }

    public async Task<ServiceResult<DepartmentDto>> UpdateDepartmentAsync(DepartmentDto departmentDto)
    {
        if (departmentDto == null)
            return ServiceResult<DepartmentDto>.Fail("Invalid request");

        if (departmentDto.Id <= 0)
            return ServiceResult<DepartmentDto>.Fail("Invalid Department ID");

        var department = _mapper.Map<Department>(departmentDto);
        var updated = await _departmentRepository.UpdateDepartmentAsync(department);
        var resultDto = _mapper.Map<DepartmentDto>(updated);

        return ServiceResult<DepartmentDto>.Success(resultDto, "Department updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteDepartmentAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid Department ID");

        var deleted = await _departmentRepository.DeleteDepartmentAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Department deleted successfully")
            : ServiceResult<bool>.Fail("Department not found");
    }
}
