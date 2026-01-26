using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.DTOs;

public class AuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public UserRole Role { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public string Password { get; set; }
    public int? EmployeeId { get; set; }
}
