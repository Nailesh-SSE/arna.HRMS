using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class RoleService : IRoleService
{
    private readonly RoleRepository _roleRepository;
    private readonly IMapper _mapper;

    public RoleService(RoleRepository roleRepository, IMapper mapper)
    {
        _roleRepository = roleRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<List<RoleDto>>> GetRoleAsync()
    {
        var result = await _roleRepository.GetRolesAsync();
        var roles = _mapper.Map<List<RoleDto>>(result);
        return ServiceResult<List<RoleDto>>.Success(roles);
    }

    public async Task<ServiceResult<RoleDto?>> GetRoleByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<RoleDto?>.Fail("Invalid Role ID");

        var result = await _roleRepository.GetRoleByIdAsync(id);
        if (result == null)
            return ServiceResult<RoleDto?>.Fail("Role not found");

        var role = _mapper.Map<RoleDto?>(result);
        return ServiceResult<RoleDto?>.Success(role);
    }

    public async Task<ServiceResult<RoleDto?>> GetRoleByNameAsync(string name)
    {
        if(string.IsNullOrWhiteSpace(name))
            return ServiceResult<RoleDto?>.Fail("Invalid Role");

        var result = await _roleRepository.GetRoleByNameAsync(name);
        if (result == null)
            return ServiceResult<RoleDto?>.Fail("Role not found"); 

        var role = _mapper.Map<RoleDto?>(result);
        return ServiceResult<RoleDto?>.Success(role);
    }

    public async Task<ServiceResult<RoleDto>> CreateRoleAsync(RoleDto dto)
    {
        if (dto == null)
            return ServiceResult<RoleDto>.Fail("Invalid request");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return ServiceResult<RoleDto>.Fail("Role name is required");

        if (await _roleRepository.RoleExistsAsync(dto.Name))
            return ServiceResult<RoleDto>.Fail("Role name already exists");

        var role = _mapper.Map<Role>(dto);

        var result = await _roleRepository.CreateRoleAsync(role);
        if (result == null)
            return ServiceResult<RoleDto>.Fail("Failed to create role");

        var createdRole = _mapper.Map<RoleDto>(result);

        return ServiceResult<RoleDto>.Success(createdRole, "Role created successfully");
    }

    public async Task<ServiceResult<RoleDto>> UpdateRoleAsync(RoleDto dto)
    {
        if (dto == null)
            return ServiceResult<RoleDto>.Fail("Invalid request");

        if (dto.Id <= 0)
            return ServiceResult<RoleDto>.Fail("Invalid Role ID");

        if (string.IsNullOrWhiteSpace(dto.Name))
            return ServiceResult<RoleDto>.Fail("Role name is required");

        var role = _mapper.Map<Role>(dto);

        var result = await _roleRepository.UpdateRoleAsync(role);
        if (result == null)
            return ServiceResult<RoleDto>.Fail("Failed to update role");

        var updatedRole = _mapper.Map<RoleDto>(result);

        return ServiceResult<RoleDto>.Success(updatedRole, "Role updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteRoleAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid Role ID");

        var result = await _roleRepository.DeleteRoleAsync(id);

        return result
            ? ServiceResult<bool>.Success(true, "Role deleted successfully")
            : ServiceResult<bool>.Fail("Role not found");
    }

    public async Task<bool> RoleExistsAsync(string role)
    {
        if (role == null)
            return false;

        return await _roleRepository.RoleExistsAsync(role);
    }  
}
