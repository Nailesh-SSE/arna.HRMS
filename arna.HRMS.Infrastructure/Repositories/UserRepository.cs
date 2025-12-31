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

    public async Task<List<User>> GetUserAsync()
    {
        return await _baseRepository.Query()
            .Include(x => x.Employee)
            .Where(x => x.IsActive && !x.IsDeleted).ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        var user = await _baseRepository.GetByIdAsync(id);
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
        var user = await _baseRepository.GetByIdAsync(id);

        if (user == null)
            return false;

        user.IsActive = false;
        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _baseRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UserExistsAsync(string username, string email)
    {
        if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(email))
            return false;

        username = username?.Trim().ToLower() ?? string.Empty;
        email = email?.Trim().ToLower() ?? string.Empty;

        return await _baseRepository.Query().AnyAsync(u =>
            u.Username.ToLower() == username ||
            u.Email.ToLower() == email);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail))
            return null;

        usernameOrEmail = usernameOrEmail.Trim().ToLower() ?? string.Empty;

        return await _baseRepository.Query()
            .FirstOrDefaultAsync(u =>
                u.Username.ToLower() == usernameOrEmail ||
                u.Email.ToLower() == usernameOrEmail);
    }

}
