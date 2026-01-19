namespace arna.HRMS.Models.DTOs;

public class RefreshTokenRequestDto
{
    public int UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
