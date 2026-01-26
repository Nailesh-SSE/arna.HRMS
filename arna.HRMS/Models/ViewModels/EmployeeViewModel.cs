using arna.HRMS.Models.Common;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels;

public class EmployeeViewModel : CommonViewModel
{
    [Display(Name = "Employee ID")]
    public string? EmployeeNumber { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public string LastName { get; set; }

    [Display(Name = "Full Name")]
    public string FullName => $"{FirstName} {LastName}";

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }

    [Required(ErrorMessage = "Phone number is required.")]
    [Phone(ErrorMessage = "Invalid phone number.")]
    [StringLength(10)]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "Date of Birth is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Required(ErrorMessage = "Hire Date is required.")]
    [DataType(DataType.Date)]
    [Display(Name = "Hire Date")]
    public DateTime HireDate { get; set; }

    [Required(ErrorMessage = "Department is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "Please select a department.")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Department")]
    public string? DepartmentName { get; set; }

    public string? DepartmentCode { get; set; }

    public int? ManagerId { get; set; }

    [Display(Name = "Manager")]
    public string? ManagerFullName { get; set; }

    [Required(ErrorMessage = "Position is required.")]
    [StringLength(100, ErrorMessage = "Position cannot exceed 100 characters.")]
    public string Position { get; set; }

    [Required(ErrorMessage = "Salary is required.")]
    [Range(1, double.MaxValue, ErrorMessage = "Salary must be greater than 0.")]
    [DataType(DataType.Currency)]
    public decimal Salary { get; set; }
}
