using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Core.DTOs.Requests;

public class CreateUserRequest
{
    [Required(ErrorMessage = "Username is required")]
    [MaxLength(100)]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    public string Username { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255)]
    public string Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
    [MaxLength(100)]
    public string Password { get; set; }

    [Required(ErrorMessage = "First name is required")]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(20)]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string PhoneNumber { get; set; }
}
