using arna.HRMS.Core.Entities;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
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
            .Include(x => x.Role)
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .Include(x => x.Employee)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);
    }

    public Task<User> CreateUserAsync(User user)
    {
        return _baseRepository.AddAsync(user);
    }

    public Task<User> UpdateUserAsync(User user)
    {
        return _baseRepository.UpdateAsync(user);
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await GetUserByIdAsync(id);
        if (user == null)
            return false;

        user.IsActive = false;
        user.IsDeleted = true;
        user.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UserExistsAsync(string email, string phoneNumber)
    {
        email = (email ?? string.Empty).Trim().ToLower();
        phoneNumber = (phoneNumber ?? string.Empty).Trim().ToLower();
        return await _baseRepository.Query()
            .FirstOrDefaultAsync(u => u.IsActive && !u.IsDeleted 
                && u.Email.Trim().ToLower() == email || u.PhoneNumber.Trim().ToLower() == phoneNumber) != null;
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail))
            return null;

        usernameOrEmail = usernameOrEmail.Trim().ToLower();

        return await _baseRepository.Query()
            .Include(x => x.Employee)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(u => u.IsActive && !u.IsDeleted &&
                (u.Username.Trim().ToLower() == usernameOrEmail ||
                 u.Email.Trim().ToLower() == usernameOrEmail));
    }

    public async Task<bool> ChangeUserPasswordAsync(int id, string newPassword)
    {
        var user = await GetUserByIdAsync(id);
        if (user == null)
            return false;

        user.PasswordHash = HashPassword(newPassword);
        user.Password = newPassword;
        user.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(user);
        return true;
    }

    private static string HashPassword(string password)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}
