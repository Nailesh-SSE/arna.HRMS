using arna.HRMS.Models.Common;

namespace arna.HRMS.Models.DTOs;

public class MonthlyAttendanceDto : CommonDto
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public string Day { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public double TotalHours { get; set; }
    public string Status { get; set; }
}
