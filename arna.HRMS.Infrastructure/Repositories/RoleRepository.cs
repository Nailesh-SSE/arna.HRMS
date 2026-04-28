using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class RoleRepository
{
    private readonly IBaseRepository<Role> _baseRepository;

    public RoleRepository(IBaseRepository<Role> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<List<Role>> GetRolesAsync()
    {
        return await _baseRepository.Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<Role?> GetRoleByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);
    }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        name = name.Trim().ToLower();

        return await _baseRepository.Query()
            .FirstOrDefaultAsync(x =>
                x.Name.Trim().ToLower() == name &&
                x.IsActive &&
                !x.IsDeleted);
    }

    public Task<Role> CreateRoleAsync(Role role)
    {
        return _baseRepository.AddAsync(role);
    }

    public Task<Role> UpdateRoleAsync(Role role)
    {
        return _baseRepository.UpdateAsync(role);
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        var role = await _baseRepository.Query()
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive &&
                !x.IsDeleted);

        if (role == null)
            return false; 

        role.IsActive = false;
        role.IsDeleted = true;
        role.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(role);
        return true;
    }

    public async Task<bool> RoleExistsAsync(string name, int? id)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        name = name.Trim().ToLower();

        return await _baseRepository.Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .AnyAsync(x =>
                x.Id != id &&
                x.Name.Trim().ToLower() == name);
    }
}