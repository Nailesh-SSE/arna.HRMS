using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class UserRepository
{
    private readonly IBaseRepository<User> _baseRepository;

    public UserRepository(IBaseRepository<User> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<IEnumerable<User>> GetUserAsync()
    {
        return await _baseRepository.Query().Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var user = await _baseRepository.GetByIdAsync(id).ConfigureAwait(false);
        return user;
    }

    public Task<User> CreateUserAsync(User User)
    {
        return _baseRepository.AddAsync(User);
    }

    public Task<User> UpdateUserAsync(User User)
    {
        return _baseRepository.UpdateAsync(User);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _baseRepository.GetByIdAsync(id).ConfigureAwait(false);

        if (user == null || user.IsDeleted)
            return false;

        user.IsActive = false;
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _baseRepository.UpdateAsync(user);
        return true;
    }

}
