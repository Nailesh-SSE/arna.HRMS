using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels;

public class EmployeeViewModel : CommonViewModel
{
    [Display(Name = "Employee ID")]
    public string? EmployeeNumber { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "First name can only contain letters.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Last name can only contain letters.")]
    public string LastName { get; set; } = string.Empty;

    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}".Trim();

    [Required(ErrorMessage = "Personal email is required.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid email address.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Office email is required.")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Please enter a valid office email address.")]
    [StringLength(100, ErrorMessage = "Office email cannot exceed 100 characters.")]
    public string OfficeEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "Phone number is required.")]
    [RegularExpression(@"^\+?[0-9]{10}$", ErrorMessage = "Phone number must be exactly 10 digits.")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Hire Date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Hire Date")]
    public DateTime HireDate { get; set; }

    [Required(ErrorMessage = "Please select a department.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a department.")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Department")]
    public string? DepartmentName { get; set; }

    public string? DepartmentCode { get; set; }

    public int? ManagerId { get; set; }

    [Display(Name = "Manager")]
    public string? ManagerFullName { get; set; }

    [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters.")]
    public string? Position { get; set; } 

    [Required(ErrorMessage = "Designation is required.")]
    public EmployeePosition? Designation { get; set; }

    [Required(ErrorMessage = "Salary is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Salary must be greater than 0.")]
    [DataType(DataType.Currency)]
    public decimal Salary { get; set; }

    public string? ProfileImage { get; set; }
}
