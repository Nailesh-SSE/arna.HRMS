using arna.HRMS.Core.Common;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.DTOs;

public class AttendanceDto : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? ClockInTime { get; set; }
    public TimeSpan? ClockOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string Notes { get; set; }
    public TimeSpan? WorkingHours { get; set; }
}
