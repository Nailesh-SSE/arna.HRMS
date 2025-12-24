using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.DTOs;

public class EmployeeDto
{
    public int Id { get; set; }

    [Display(Name = "Employee ID")]
    public string EmployeeNumber { get; set; }

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

    [Phone]
    public string PhoneNumber { get; set; }

    [Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }

    [Display(Name = "Hire Date")]
    public DateTime HireDate { get; set; }

    // Department Information (Flattened for simplicity)
    public int DepartmentId { get; set; }
    [Display(Name = "Department")]
    public string DepartmentName { get; set; }
    public string DepartmentCode { get; set; }

    // Manager Information (Flattened for simplicity)
    public int? ManagerId { get; set; }
    [Display(Name = "Manager")]
    public string ManagerFullName { get; set; }

    [StringLength(100)]
    public string Position { get; set; }

    [DataType(DataType.Currency)]
    public decimal Salary { get; set; }

    [Display(Name = "Status")]
    public bool IsActive { get; set; }

    // Optional: Auditing fields if needed in the response
    public DateTime CreatedAt { get; set; }
}
