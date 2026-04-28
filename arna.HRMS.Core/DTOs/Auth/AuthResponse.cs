namespace arna.HRMS.Core.DTOs.Auth;

public class AuthResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiration { get; set; }
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string FullName { get; set; }
    public string Role { get; set; }
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public int? EmployeeId { get; set; }
}
