using arna.HRMS.Models.Common;
using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace arna.HRMS.Models.DTOs;

public class AttendanceRequestDto : CommonDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public DateTime Date { get; set; }

    [Required(ErrorMessage ="Reason is required.")]
    public AttendanceReasonType ReasonType { get; set; }

    [Required(ErrorMessage = "Select your Location")]
    public AttendanceLocation Location { get; set; }

    [Required(ErrorMessage = "Select your Clock In time")]
    public DateTime? ClockIn { get; set; }

    [Required(ErrorMessage = "Select your Clock out time")]
    public DateTime? ClockOut { get; set; }

    public TimeSpan BreakDuration { get; set; }
    public TimeSpan TotalHours { get; set; }
    public string? Description { get; set; }
    public bool IsApproved { get; set; } = false;
    public DateTime? ApprovedBy { get; set; }
}
