using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class DepartmentService : IDepartmentService
{
    private readonly DepartmentRepository _repository;
    private readonly IMapper _mapper;
    private readonly DepartmentValidator _validator;

    public DepartmentService(
        DepartmentRepository repository,
        IMapper mapper,
        DepartmentValidator validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ServiceResult<List<DepartmentDto>>> GetDepartmentsAsync()
    {
        var departments = await _repository.GetDepartmentsAsync();
        var dtoList = _mapper.Map<List<DepartmentDto>>(departments);
        var response = ServiceResult<List<DepartmentDto>>.Success(dtoList);
        return response;
    }

    public async Task<ServiceResult<DepartmentDto?>> GetDepartmentByIdAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<DepartmentDto?>.Fail("Invalid department ID");
            return failId;
        }

        var department = await _repository.GetDepartmentByIdAsync(id);
        if (department == null)
        {
            var failNotFound = ServiceResult<DepartmentDto?>.Fail("Department not found");
            return failNotFound;
        }

        var dto = _mapper.Map<DepartmentDto>(department);
        if (dto == null)
        {
            var failMap = ServiceResult<DepartmentDto?>.Fail("Failed to Find department.");
            return failMap;
        }

        var response = ServiceResult<DepartmentDto?>.Success(dto);
        return response;
    }

    public async Task<ServiceResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<DepartmentDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var entity = _mapper.Map<Department>(dto);
        var created = await _repository.CreateDepartmentAsync(entity);
        var mapped = _mapper.Map<DepartmentDto>(created);
        if (mapped == null)
        {
            var failMap = ServiceResult<DepartmentDto>.Fail("Failed to create department.");
            return failMap;
        }

        var response = ServiceResult<DepartmentDto>.Success(mapped, "Department created successfully.");
        return response;
    }

    public async Task<ServiceResult<DepartmentDto>> UpdateDepartmentAsync(DepartmentDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<DepartmentDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var entity = _mapper.Map<Department>(dto);
        var updated = await _repository.UpdateDepartmentAsync(entity);
        var mapped = _mapper.Map<DepartmentDto>(updated);
        if (mapped == null)
        {
            var failMap = ServiceResult<DepartmentDto>.Fail("Failed to Update department.");
            return failMap;
        }

        var response = ServiceResult<DepartmentDto>.Success(mapped, "Department updated successfully.");
        return response;
    }

    public async Task<ServiceResult<bool>> DeleteDepartmentAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<bool>.Fail("Department not found");
            return failId;
        }

        var deleted = await _repository.DeleteDepartmentAsync(id);
        if (deleted)
        {
            var success = ServiceResult<bool>.Success(true, "Department deleted successfully.");
            return success;
        }

        var failNotFound = ServiceResult<bool>.Fail("Department not found");
        return failNotFound;
    }
}
