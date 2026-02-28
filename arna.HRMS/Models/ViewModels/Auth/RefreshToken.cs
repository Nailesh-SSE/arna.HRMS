namespace arna.HRMS.Models.ViewModels.Auth;

public class RefreshToken
{
    public int UserId { get; set; }
    public string Token { get; set; } = string.Empty; 
}
