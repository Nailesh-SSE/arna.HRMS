using arna.HRMS.Core.Common.Base;

namespace arna.HRMS.Core.DTOs;

public class EmployeeDto : BaseEntity
{
    public string? EmployeeNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public int? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public string? DepartmentCode { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerFullName { get; set; }
    public string Position { get; set; }
    public decimal Salary { get; set; }
    public string FullName => $"{FirstName} {LastName}";
}
