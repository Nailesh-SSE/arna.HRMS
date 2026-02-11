using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
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

    public async Task<ServiceResult<List<RoleDto>>> GetRoleAsync()
    {
        var roles = await _repository.GetRolesAsync();
        return ServiceResult<List<RoleDto>>.Success(_mapper.Map<List<RoleDto>>(roles));
    }

    public async Task<ServiceResult<RoleDto?>> GetRoleByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<RoleDto?>.Fail("Invalid Role ID");

        var role = await _repository.GetRoleByIdAsync(id);

        if (role == null)
            return ServiceResult<RoleDto?>.Fail("Role not found");

        return ServiceResult<RoleDto?>.Success(_mapper.Map<RoleDto>(role));
    }

    public async Task<ServiceResult<RoleDto?>> GetRoleByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return ServiceResult<RoleDto?>.Fail("Role name is required");

        var role = await _repository.GetRoleByNameAsync(name);
        if (role == null)
            return ServiceResult<RoleDto?>.Fail("Role not found");

        return ServiceResult<RoleDto?>.Success(_mapper.Map<RoleDto>(role));
    }

    public async Task<ServiceResult<RoleDto>> CreateRoleAsync(RoleDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<RoleDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<Role>(dto);
        var created = await _repository.CreateRoleAsync(entity);

        return ServiceResult<RoleDto>.Success(_mapper.Map<RoleDto>(created), "Role created successfully");
    }

    public async Task<ServiceResult<RoleDto>> UpdateRoleAsync(RoleDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<RoleDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var updated = await _repository.UpdateRoleAsync(_mapper.Map<Role>(dto));

        return ServiceResult<RoleDto>.Success(_mapper.Map<RoleDto>(updated), "Role updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteRoleAsync(int id)
    {
        var role = await GetRoleByIdAsync(id);
        if (!role.IsSuccess)
            return ServiceResult<bool>.Fail("Role not found");

        var deleted = await _repository.DeleteRoleAsync(id);

        return ServiceResult<bool>.Success(deleted, "Role deleted successfully");
    }
}
