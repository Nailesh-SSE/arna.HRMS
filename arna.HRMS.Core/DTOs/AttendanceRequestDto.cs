using arna.HRMS.Core.Common.Base;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.DTOs;

public class AttendanceRequestDto : BaseEntity
{
    public int EmployeeId { get; set; }
    public string? EmployeeName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public AttendanceReasonType? ReasonTypeId { get; set; }
    public AttendanceLocation? LocationId { get; set; }
    public DateTime? ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public TimeSpan? BreakDuration { get; set; }
    public TimeSpan TotalHours { get; set; }
    public string? Description { get; set; }
    public Status StatusId { get; set; } = Status.Pending;
    public DateTime? ApprovedOn { get; set; }
    public int? ApprovedBy { get; set; }
    public string? ApprovedByName { get; set; }
}
