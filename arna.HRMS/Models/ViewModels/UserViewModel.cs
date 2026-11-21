using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels;

public class UserViewModel : CommonViewModel
{
    [Required(ErrorMessage = "Username is required.")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters.")]
    [MaxLength(100, ErrorMessage = "Username cannot exceed 100 characters.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email address.")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters.")]
    public string Email { get; set; } = string.Empty;

    [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
    [MaxLength(100, ErrorMessage = "Password cannot exceed 100 characters.")]
    public string? Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required.")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a role.")]
    public int? RoleId { get; set; }

    public string? Role { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\+?[0-9\s\-]{7,15}$", ErrorMessage = "Please enter a valid phone number (7-15 digits).")]
    public string? PhoneNumber { get; set; }

    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public string? PasswordHash { get; set; }
    public string? ProfileImage { get; set; }
}
