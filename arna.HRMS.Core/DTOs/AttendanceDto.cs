using arna.HRMS.Core.Common.Base;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.DTOs;

public class AttendanceDto : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan? ClockInTime { get; set; }
    public TimeSpan? ClockOutTime { get; set; }
    public AttendanceStatus StatusId { get; set; }
    public string Notes { get; set; }
    public Double? Latitude { get; set; }
    public Double? Longitude { get; set; }
    public TimeSpan? WorkingHours { get; set; }
    public string? Device { get; set; }

}
