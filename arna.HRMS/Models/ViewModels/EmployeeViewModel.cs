using arna.HRMS.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels;

public class EmployeeViewModel
{
    // Employee basic info
    public EmployeeDto Employee { get; set; }

    public int Id { get; set; }
    [Required(ErrorMessage = "Employee Number is required")]
    [Display(Name = "Employee ID")]
    public string EmployeeNumber { get; set; }

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; }
    [Required(ErrorMessage ="Phone number is required")]
    [Phone(ErrorMessage ="Invalid Phone number")]
    public string PhoneNumber { get; set; }
    [Required(ErrorMessage = "Date of Birth is required")]
    //[Display(Name = "Date of Birth")]
    public DateTime DateOfBirth { get; set; }
    [Required(ErrorMessage = "Hire Date is required")]
    //[Display(Name = "HireDate")]
    public DateTime HireDate { get; set; }
    [Required]
    public int DepartmentId { get; set; }
    [Display(Name = "Department")]
    public string DepartmentName { get; set; }
    public string DepartmentCode { get; set; }
    public int? ManagerId { get; set; }
    public string ManagerFullName { get; set; }
    [Required(ErrorMessage ="Position is required")]
    [StringLength(100)]
    public string Position { get; set; }
    [Required(ErrorMessage ="Salary is required")]
    [DataType(DataType.Currency)]
    [Range(1, 99999999, ErrorMessage = "Salary must be greater than 0")]
    public decimal Salary { get; set; }
    [Display(Name = "Status")]
    public bool IsActive { get; set; }

    // Attendance history
    //public List<AttendanceDto> Attendances { get; set; } = new();
    // Leave requests
    //public List<LeaveRequestDto> LeaveRequests { get; set; } = new();
    // Timesheets
    //public List<TimesheetDto> Timesheets { get; set; } = new();
}
