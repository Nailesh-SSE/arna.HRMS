using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly RoleRepository _repository;
    private readonly IMapper _mapper;
    private readonly RoleValidator _validator;

    public RoleService(
        RoleRepository repository,
        IMapper mapper,
        RoleValidator validator)
    {
        _repository = repository;
        _mapper = mapper;
        _validator = validator;
    }

    public async Task<ServiceResult<List<RoleDto>>> GetRolesAsync()
    {
        var roles = await _repository.GetRolesAsync();
        var dtos = _mapper.Map<List<RoleDto>>(roles);
        var response = ServiceResult<List<RoleDto>>.Success(dtos);
        return response;
    }

    public async Task<ServiceResult<RoleDto?>> GetRoleByIdAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<RoleDto?>.Fail("Invalid Role ID");
            return failId;
        }

        var role = await _repository.GetRoleByIdAsync(id);
        if (role == null)
        {
            var failNotFound = ServiceResult<RoleDto?>.Fail("Role not found");
            return failNotFound;
        }

        var dto = _mapper.Map<RoleDto>(role);
        if (dto == null)
        {
            var failMap = ServiceResult<RoleDto?>.Fail("No Data Found");
            return failMap;
        }

        var response = ServiceResult<RoleDto?>.Success(dto);
        return response;
    }

    public async Task<ServiceResult<RoleDto?>> GetRoleByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            var failName = ServiceResult<RoleDto?>.Fail("Role name is required");
            return failName;
        }

        var role = await _repository.GetRoleByNameAsync(name.Trim());
        if (role == null)
        {
            var failNotFound = ServiceResult<RoleDto?>.Fail("Role not found");
            return failNotFound;
        }

        var dto = _mapper.Map<RoleDto>(role);
        if (dto == null)
        {
            var failMap = ServiceResult<RoleDto?>.Fail("No Data Found");
            return failMap;
        }

        var response = ServiceResult<RoleDto?>.Success(dto);
        return response;
    }

    public async Task<ServiceResult<RoleDto>> CreateRoleAsync(RoleDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<RoleDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var entity = _mapper.Map<Role>(dto);
        var created = await _repository.CreateRoleAsync(entity);
        var mapped = _mapper.Map<RoleDto>(created);
        if (mapped == null)
        {
            var failMap = ServiceResult<RoleDto>.Fail("Fail to create Role");
            return failMap;
        }

        var response = ServiceResult<RoleDto>.Success(mapped, "Role created successfully");
        return response;
    }

    public async Task<ServiceResult<RoleDto>> UpdateRoleAsync(RoleDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<RoleDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var entity = _mapper.Map<Role>(dto);
        var updated = await _repository.UpdateRoleAsync(entity);
        if (updated == null)
        {
            var failNull = ServiceResult<RoleDto>.Fail("No Data Found");
            return failNull;
        }

        var mapped = _mapper.Map<RoleDto>(updated);
        if (mapped == null)
        {
            var failMap = ServiceResult<RoleDto>.Fail("Fail to Update Role");
            return failMap;
        }

        var response = ServiceResult<RoleDto>.Success(mapped, "Role updated successfully");
        return response;
    }

    public async Task<ServiceResult<bool>> DeleteRoleAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<bool>.Fail("Role not found");
            return failId;
        }

        var deleted = await _repository.DeleteRoleAsync(id);
        if (deleted)
        {
            var success = ServiceResult<bool>.Success(true, "Role deleted successfully");
            return success;
        }

        var failNotFound = ServiceResult<bool>.Fail("Role not found");
        return failNotFound;
    }
}
