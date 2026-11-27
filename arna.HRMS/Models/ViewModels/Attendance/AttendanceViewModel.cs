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
    public AttendanceStatus StatusId { get; set; }

    [Required(ErrorMessage = "Note is required.")]
    [StringLength(500, ErrorMessage = "Note cannot exceed 500 characters.")]
    public string Notes { get; set; } = string.Empty;

    public Double? Latitude { get; set; }
    public Double? Longitude { get; set; }
    public TimeSpan? WorkingHours { get; set; }
    public DeviceType? DeviceId { get; set; }
}
