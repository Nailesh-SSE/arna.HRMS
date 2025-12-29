using arna.HRMS.Models.Enums;

namespace arna.HRMS.Models.ViewModels;

public class AttendanceViewModel
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public AttendanceStatuses Status { get; set; }
    public string Notes { get; set; }
    public double WorkingHours { get; set; }
}
