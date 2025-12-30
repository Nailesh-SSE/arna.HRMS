using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.DTOs.Requests;

public class UpdateAttendanceRequest
{
    public int Id { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public AttendanceStatus Status { get; set; }
    public string Notes { get; set; }
    public double WorkingHours { get; set; }
}
