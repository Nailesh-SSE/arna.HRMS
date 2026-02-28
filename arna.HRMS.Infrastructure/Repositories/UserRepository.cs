using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace arna.HRMS.Infrastructure.Repositories;

public class UserRepository
{
    private readonly IBaseRepository<User> _baseRepository;

    public UserRepository(IBaseRepository<User> baseRepository)
    {
        _baseRepository = baseRepository;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _baseRepository.Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .Include(x => x.Employee)
            .Include(x => x.Role)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<User?> GetUserByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .Where(x => x.Id == id && x.IsActive && !x.IsDeleted)
            .Include(x => x.Employee)
            .Include(x => x.Role)
            .FirstOrDefaultAsync();
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
        var user = await _baseRepository.Query()
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive &&
                !x.IsDeleted);

        if (user == null)
            return false; 

        user.IsActive = false;
        user.IsDeleted = true;
        user.UpdatedOn = DateTime.Now;

        await _baseRepository.UpdateAsync(user);
        return true;
    }

    public async Task<bool> UserExistsAsync(string? email, string? phoneNumber, int? id)
    {
        if (string.IsNullOrWhiteSpace(email) &&
            string.IsNullOrWhiteSpace(phoneNumber))
            return false;

        email = email?.Trim().ToLower();
        phoneNumber = phoneNumber?.Trim();

        return await _baseRepository.Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .AnyAsync(x =>
                x.Id != id &&
                (
                    (!string.IsNullOrEmpty(email) && x.Email.Trim().ToLower() == email) ||
                    (!string.IsNullOrEmpty(phoneNumber) && x.PhoneNumber.Trim().ToLower() == phoneNumber)
                )
            );
    }

    public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
    {
        if (string.IsNullOrWhiteSpace(usernameOrEmail))
            return null;

        usernameOrEmail = usernameOrEmail.Trim().ToLower();

        return await _baseRepository.Query()
            .Where(x => x.IsActive && !x.IsDeleted)
            .Include(x => x.Employee)
            .Include(x => x.Role)
            .FirstOrDefaultAsync(x =>
                x.Username.Trim().ToLower() == usernameOrEmail ||
                x.Email.Trim().ToLower() == usernameOrEmail);
    }

    public async Task<bool> ChangeUserPasswordAsync(int id, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(newPassword))
            return false;

        var user = await _baseRepository.Query()
            .FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.IsActive &&
                !x.IsDeleted);

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
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToBase64String(hash);
    }
}