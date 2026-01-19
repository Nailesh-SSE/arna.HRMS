using arna.HRMS.Core.Common;

namespace arna.HRMS.Core.Entities;

public class AttendanceRequest : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public string ReasonType { get; set; } 
    public string Location { get; set; }
    public string? Description { get; set; }
    public DateTime? ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan TotalHours { get; set; }
    public bool IsApproved { get; set; }=false;
    public DateTime? ApprovedBy { get; set; }
    public Employee Employee { get; set; }
}
