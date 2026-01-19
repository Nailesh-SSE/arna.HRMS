using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;

namespace arna.HRMS.Models.DTOs;

public class AttendanceDto : CommonDto
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? ClockInTime { get; set; }
    public TimeSpan? ClockOutTime { get; set; }
    public AttendanceStatuses Status { get; set; }
    public string Notes { get; set; }
    public TimeSpan? WorkingHours { get; set; }
}
