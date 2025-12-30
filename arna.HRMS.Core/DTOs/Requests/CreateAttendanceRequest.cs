using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.DTOs.Requests;

public class CreateAttendanceRequest
{

    public DateTime Date { get; set; }
    public int EmployeeId { get; set; }
    public TimeSpan? ClockInTime { get; set; }
    public TimeSpan? ClockOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string Notes { get; set; }
    public double WorkingHours { get; set; }
}
