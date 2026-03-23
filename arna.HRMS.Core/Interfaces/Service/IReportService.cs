using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IReportService
{
    Task<ServiceResult<List<AttendanceReportDto>>> GetDailyAttendanceReport(int? year, int? month, int? employeeId, AttendanceStatus? statusId, DeviceType? device, DateTime? FromDate, DateTime? ToDate);
    Task<ServiceResult<List<EmployeeAttendanceReportDto>>> GetEmployeeAttendanceReport(int? year, int? month, int? employeeId, DateTime? FromDate, DateTime? ToDate);
    Task<ServiceResult<List<LeaveSummaryReportDto>>> GetLeaveSummaryReport(int year, int? month, int? departmentId, DateTime? FromDate, DateTime? ToDate);

}
