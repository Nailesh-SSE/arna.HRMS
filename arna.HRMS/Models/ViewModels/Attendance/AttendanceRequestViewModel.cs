using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.ViewModels.Attendance;

public class AttendanceRequestViewModel : CommonViewModel
{
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }

    [Required(ErrorMessage = "FromDate is required.")]
    public DateTime? FromDate { get; set; }

    [Required(ErrorMessage = "ToDate is required.")]
    public DateTime? ToDate { get; set; }

    [Required(ErrorMessage = "Reason is required.")]
    public AttendanceReasonType? ReasonType { get; set; }

    [Required(ErrorMessage = "Select your Location")]
    public AttendanceLocation? Location { get; set; }

    [Required(ErrorMessage = "Select your Clock In time")]
    public DateTime? ClockIn { get; set; }

    [Required(ErrorMessage = "Select your Clock out time")]
    public DateTime? ClockOut { get; set; }

    [Required(ErrorMessage = "Break duration is required")]
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan TotalHours { get; set; }

    [Required(ErrorMessage = "Message is required")]
    public string? Description { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovedBy { get; set; }
}
