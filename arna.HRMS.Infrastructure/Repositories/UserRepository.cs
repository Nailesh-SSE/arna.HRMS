using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Interfaces;

namespace arna.HRMS.Infrastructure.Repositories;

public class UserRepository
{
    private readonly IBaseRepository<User> _baseRepository;

    public UserRepository(IBaseRepository<User> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public Task<IEnumerable<User>> GetUserAsync()
    {
        return _baseRepository.GetAllAsync();
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

    public Task<bool> DeleteUserAsync(int id)
    {
        return _baseRepository.DeleteAsync(id);
    }
}
