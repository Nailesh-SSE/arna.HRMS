using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels.Auth;

public class LoginViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required.")]
    public string Password { get; set; } = string.Empty; 
}
