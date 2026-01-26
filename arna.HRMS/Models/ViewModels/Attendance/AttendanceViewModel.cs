using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels.Attendance;

public class AttendanceViewModel : CommonViewModel
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? ClockInTime { get; set; }
    public TimeSpan? ClockOutTime { get; set; }
    public AttendanceStatuses Status { get; set; }

    [Required(ErrorMessage = "Note is required")]
    public string Notes { get; set; }

    public TimeSpan? WorkingHours { get; set; }
}
