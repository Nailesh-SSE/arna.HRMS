using arna.HRMS.Models.Enums;

namespace arna.HRMS.Models.ViewModels;

public class ReportFilter
{
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? EmployeeId { get; set; }
    public int? DepartmentId { get; set; }
    public DeviceType? Device { get; set; }
    public AttendanceStatus? Status { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}