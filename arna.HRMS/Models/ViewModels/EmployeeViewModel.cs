using arna.HRMS.Models.DTOs;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels;

public class EmployeeViewModel
{
    // Employee basic info
    public EmployeeDto Employee { get; set; }

    public int Id { get; set; }
    public string EmployeeNumber { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public DateTime DateOfBirth { get; set; }
    public DateTime HireDate { get; set; }
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; }
    public string DepartmentCode { get; set; }
    public int? ManagerId { get; set; }
    public string ManagerFullName { get; set; }
    public string Position { get; set; }
    public decimal Salary { get; set; }
    public bool IsActive { get; set; }

    // Attendance history
    //public List<AttendanceDto> Attendances { get; set; } = new();
    // Leave requests
    //public List<LeaveRequestDto> LeaveRequests { get; set; } = new();
    // Timesheets
    //public List<TimesheetDto> Timesheets { get; set; } = new();
}
