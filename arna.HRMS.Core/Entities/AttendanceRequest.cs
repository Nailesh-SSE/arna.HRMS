using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Entities;

public class AttendanceRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public AttendanceReasonType ReasonType { get; set; }
    public AttendanceLocation Location { get; set; }
    public string? Description { get; set; }
    public DateTime? ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public TimeSpan BreakDuration { get; set; }
    public TimeSpan TotalHours { get; set; }
    public bool IsApproved { get; set; }=false;
    public DateTime? ApprovedBy { get; set; }


    public Employee Employee { get; set; }
}
