using arna.HRMS.Core.Common;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Entities;

public class AttendanceRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public AttendanceReasonType ReasonType { get; set; } 
    public AttendanceLocation Location { get; set; }
    public string? Description { get; set; }
    public DateTime? ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan TotalHours { get; set; }
    public Status Status { get; set; } = Status.Pending;
    public DateTime? ApprovedOn { get; set; }
    public int? ApprovedBy { get; set; }
    public Employee Employee { get; set; }
    public Employee ApprovedByEmployee { get; set; }
}
