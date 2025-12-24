using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Entities;

public class Attendance : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public TimeSpan? TotalHours { get; set; }
    public AttendanceStatus Status { get; set; }
    public string Notes { get; set; }

    // Navigation Properties
    public Employee Employee { get; set; }
}
