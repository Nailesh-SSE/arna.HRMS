using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
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

    public async Task<ServiceResult<List<DepartmentDto>>> GetDepartmentAsync()
    {
        var departments = await _repository.GetDepartmentAsync();
        return ServiceResult<List<DepartmentDto>>.Success(_mapper.Map<List<DepartmentDto>>(departments));
    }

    public async Task<ServiceResult<DepartmentDto?>> GetDepartmentByIdAsync(int id)
    {
        if(id <= 0)
            return ServiceResult<DepartmentDto?>.Fail("Invalid department ID");

        var department = await _repository.GetDepartmentByIdAsync(id);
        if (department == null)
            return ServiceResult<DepartmentDto?>.Fail("Department not found");

        return ServiceResult<DepartmentDto?>.Success(_mapper.Map<DepartmentDto>(department));
    }

    public async Task<ServiceResult<DepartmentDto>> CreateDepartmentAsync(DepartmentDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<DepartmentDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<Department>(dto);
        var created = await _repository.CreateDepartmentAsync(entity);

        return ServiceResult<DepartmentDto>.Success(_mapper.Map<DepartmentDto>(created), "Department created successfully");
    }

    public async Task<ServiceResult<DepartmentDto>> UpdateDepartmentAsync(DepartmentDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<DepartmentDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var updated = await _repository.UpdateDepartmentAsync(_mapper.Map<Department>(dto));

        return ServiceResult<DepartmentDto>.Success(_mapper.Map<DepartmentDto>(updated), "Department updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteDepartmentAsync(int id)
    {
        var department = await GetDepartmentByIdAsync(id);
        if (!department.IsSuccess || department.Data == null)
            return ServiceResult<bool>.Fail("Department not found");

        var deleted = await _repository.DeleteDepartmentAsync(id);

        return ServiceResult<bool>.Success(deleted, "Department deleted successfully");
    }
}
