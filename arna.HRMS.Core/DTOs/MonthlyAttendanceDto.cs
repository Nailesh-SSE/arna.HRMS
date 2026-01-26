using arna.HRMS.Core.Common;

namespace arna.HRMS.Core.DTOs;

public class MonthlyAttendanceDto : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public string Day { get; set; }
    public TimeSpan? ClockIn { get; set; }
    public TimeSpan? ClockOut { get; set; }
    public TimeSpan TotalHours { get; set; }
    public string Status { get; set; }
}
