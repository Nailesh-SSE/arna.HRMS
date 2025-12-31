using BlazorBootstrap;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace arna.HRMS.Models.DTOs;

public class MonthlyAttendanceDto
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public string Day { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public double TotalHours { get; set; }
    public string Status { get; set; }
}
