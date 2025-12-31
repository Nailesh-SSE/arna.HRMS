using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.DTOs;

public class UserDto : BaseEntity
{
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100)]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50)]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Full Name")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required")]
    public UserRole? Role { get; set; }

    [Required(ErrorMessage = "Phone Number is required")]
    [Phone(ErrorMessage = "Invalid phone number")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Employee is required")]
    public int? EmployeeId { get; set; }

    public string? EmployeeName { get; set; }
    public string? PasswordHash { get; set; }
}
