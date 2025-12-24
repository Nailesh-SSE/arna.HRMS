using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Core.DTOs.Requests;

public class UpdateEmployeeRequest
{
    public int Id { get; set; }
    [Required(ErrorMessage = "Employee Number is required.")]
    [StringLength(10)]
    public string EmployeeNumber { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50)]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50)]
    public string LastName { get; set; }

    [StringLength(100)]
    public string Position { get; set; }

    [DataType(DataType.Date)]
    [Required(ErrorMessage = "Date of Birth is required.")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Hire Date is required.")]
    [DataType(DataType.Date)]
    public DateTime HireDate { get; set; }

    [Required(ErrorMessage = "Salary is required.")]
    [Range(0, 999999999.99, ErrorMessage = "Salary must be a positive value.")]
    public decimal Salary { get; set; }

    [Required(ErrorMessage = "Department ID is required.")]
    public int DepartmentId { get; set; }

    public int? ManagerId { get; set; } // Nullable

    // --- User Account Fields ---

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(100)]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [StringLength(20)]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string PhoneNumber { get; set; }

    //[Required(ErrorMessage = "Password is required.")]
    //[MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
    //[DataType(DataType.Password)]
    //public string Password { get; set; }

    //[Required(ErrorMessage = "User Role is required.")]
    //// Uses the UserRoles enum from the Core project for type safety
    //public UserRole PrimaryRole { get; set; } = UserRole.Employee;
}
