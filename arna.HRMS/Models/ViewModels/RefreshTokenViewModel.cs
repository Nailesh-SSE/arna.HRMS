namespace arna.HRMS.Models.ViewModels;

public class RefreshTokenViewModel
{
    public int UserId { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
}
