using arna.HRMS.Core.Common;
using System.ComponentModel.DataAnnotations.Schema;

namespace arna.HRMS.Core.Entities;

public class Employee : BaseEntity
{
    public string EmployeeNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public int DepartmentId { get; set; }
    public int? ManagerId { get; set; }
    public string Position { get; set; }
    public decimal Salary { get; set; }

    [NotMapped]
    public string FullName => $"{FirstName} {LastName}";

    // Navigation Properties
    public Department Department { get; set; }
    public Employee Manager { get; set; }
    public ICollection<Employee> Subordinates { get; set; }
    public ICollection<Attendance> Attendances { get; set; }
    public ICollection<LeaveRequest> LeaveRequests { get; set; }
    public ICollection<Timesheet> Timesheets { get; set; }
    public ICollection<AttendanceRequest> AttendanceRequest { get; set; }
    public ICollection<AttendanceRequest> AttendanceRequestsApproved { get; set; }
    public ICollection<LeaveRequest> ApprovedLeaveRequests { get; set; }
    public ICollection<EmployeeLeaveBalance> EmployeeLeaveBalance { get; set; }
}
