using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services.Report;

public interface IReportService
{
    Task<ApiResult<List<EmployeeAttendanceReportViewModel>>> GetEmployeeAttendanceReportAsync(int? year, int? month, int? employeeId, DateTime? FromDate, DateTime? ToDate);
    Task<ApiResult<List<AttendanceReportViewModel>>> GetEmployeeDailyAttendanceReportAsync(int? year, int? month, int? employeeId, AttendanceStatus? StatusId, DeviceType? device, DateTime? FromDate, DateTime? ToDate);
    Task<ApiResult<List<LeaveSummaryReportViewModel>>> GetEmployeeLeaveSummaryReportAsync(int? year, int? month, int? departmentId, DateTime? FromDate, DateTime? ToDate);
    Task<ApiResult<List<EmployeeLeaveDetailsReportViewModel>>> GetEmployeeLeaveDetailsReportAsync(int? year, int? month, DateTime? FromDate, DateTime? ToDate, int? employeeId, string? employeeNumber);
}

public class ReportService : IReportService
{
    private readonly ApiClients.ReportApi _report;

    public ReportService(ApiClients api)
    {
        _report = api.Report;
    }

    public async Task<ApiResult<List<AttendanceReportViewModel>>> GetEmployeeDailyAttendanceReportAsync(int? year, int? month, int? employeeId, AttendanceStatus? statusId, DeviceType? device, DateTime? FromDate, DateTime? ToDate)
    {
        return await _report.GetEmployeesDailyAttendanceReportAsync(year, month, employeeId, statusId, device, FromDate, ToDate);
    }
    public async Task<ApiResult<List<EmployeeAttendanceReportViewModel>>> GetEmployeeAttendanceReportAsync(int? year, int? month, int? employeeId, DateTime? FromDate, DateTime? ToDate)
    {
        return await _report.GetEmployeesAttendanceReportAsync(year, month, employeeId, FromDate, ToDate);
    }
    public async Task<ApiResult<List<LeaveSummaryReportViewModel>>> GetEmployeeLeaveSummaryReportAsync(int? year, int? month, int? departmentId, DateTime? FromDate, DateTime? ToDate)
    {
        return await _report.GetLeaveSummaryReportAsync(year, month, departmentId, FromDate, ToDate);
    }
    public async Task<ApiResult<List<EmployeeLeaveDetailsReportViewModel>>> GetEmployeeLeaveDetailsReportAsync(int? year, int? month, DateTime? FromDate, DateTime? ToDate, int? employeeId, string? employeeNumber)
    {
        return await _report.GetEmployeeLeaveDetailsReportAsync(year, month, employeeId, employeeNumber, FromDate, ToDate);
    }
}
