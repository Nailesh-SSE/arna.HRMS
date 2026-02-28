using arna.HRMS.Core.Common.Base;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Entities;

public class Attendance : BaseEntity
{
    public int EmployeeId { get; set; }
    public DateTime Date { get; set; }
    public DateTime? ClockIn { get; set; }
    public DateTime? ClockOut { get; set; }
    public TimeSpan? TotalHours { get; set; }
    public AttendanceStatus StatusId { get; set; }
    public string Notes { get; set; }
    public Double? Latitude { get; set; }
    public Double? Longitude { get; set; }
    public string? Device { get; set; }

    public Employee Employee { get; set; }
}
