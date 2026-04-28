namespace arna.HRMS.Core.DTOs;

public class MonthlyAttendanceDto
{
    public DateOnly Date { get; set; }
    public string Day { get; set; }
    public List<EmployeeDailyAttendanceDto> Employees { get; set; } = new();
}
    
