using arna.HRMS.Core.Common.Base;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace arna.HRMS.Core.Entities;

public class User : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string Username { get; set; }

    [Required]
    [MaxLength(255)]
    public string Email { get; set; }

    [Required]
    [MaxLength(255)]
    public string PasswordHash { get; set; }

    [Required]
    [MaxLength(50)]
    public string FirstName { get; set; }

    [Required]
    [MaxLength(50)]
    public string LastName { get; set; }

    [Required]
    [MaxLength(20)]
    public string PhoneNumber { get; set; }

    [Required]
    public int RoleId { get; set; } 

    [Required]
    [MaxLength(50)]
    public string Password { get; set; } 

    [MaxLength(255)]
    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    public int? EmployeeId { get; set; }  

    public Employee? Employee { get; set; }
    public Role? Role { get; set; }
}
