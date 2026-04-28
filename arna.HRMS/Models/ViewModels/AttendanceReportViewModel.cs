using arna.HRMS.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace arna.HRMS.Models.ViewModels;

public class AttendanceReportViewModel
{
    public int EmployeeId { get; set; }

    public string? EmployeeNumber { get; set; }

    public string? EmployeeName { get; set; }

    public string? Department { get; set; }

    public DateTime AttendanceDate { get; set; }

    public string? Day { get; set; }

    public TimeSpan? ClockIn { get; set; }

    public TimeSpan? ClockOut { get; set; }

    public TimeSpan? WorkingHours { get; set; }

    public TimeSpan? TotalHours { get; set; }

    public TimeSpan? BreakDuration { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    // Device comes from SQL as "1,2"
    public string? Device { get; set; }

    public AttendanceStatus? AttendanceStatus { get; set; }

    [JsonIgnore]
    [NotMapped]
    public List<DeviceType>? Devices =>
        Device?
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => (DeviceType)int.Parse(x.Trim()))
            .ToList();
}