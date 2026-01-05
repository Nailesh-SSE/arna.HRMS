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
        user.UpdatedBy = DateTime.Now;

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
            u.Username.Trim().ToLower() == username ||
            u.Email.Trim().ToLower() == email);
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail))
            return null;

        usernameOrEmail = usernameOrEmail.Trim().ToLower() ?? string.Empty;

        return await _baseRepository.Query()
            .Include(x => x.Employee)
            .FirstOrDefaultAsync(u =>
                u.Username.Trim().ToLower() == usernameOrEmail ||
                u.Email.Trim().ToLower() == usernameOrEmail);
    }
    public async Task<bool> ChangeUserPasswordAsync(int id, string newPassword)
    {
        var user = await _baseRepository.GetByIdAsync(id);
        if (user == null)
            return false;
        user.Password = newPassword;
        user.UpdatedBy = DateTime.Now;
        user.PasswordHash = HashPassword(newPassword);
        await _baseRepository.UpdateAsync(user);
        return true;
    }
    private string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
