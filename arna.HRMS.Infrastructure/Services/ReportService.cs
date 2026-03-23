using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;

namespace arna.HRMS.Infrastructure.Services;

public class ReportService : IReportService
{
    private readonly ReportRepository _reportRepository;
    
    public ReportService(ReportRepository reportRepository)
    {
        _reportRepository = reportRepository;
    }

    public async Task<ServiceResult<List<AttendanceReportDto>>> GetDailyAttendanceReport(int? year, int? month, int? employeeId, AttendanceStatus? statusId, DeviceType? device, DateTime? FromDate, DateTime? ToDate)
    {
        var reportData = await _reportRepository.GetDailyAttendanceReportAsync(year, month, employeeId, statusId, device, FromDate, ToDate);
        if (reportData == null || !reportData.Any())
            return ServiceResult<List<AttendanceReportDto>>.Fail("No attendance data found for the specified criteria.");

        return ServiceResult<List<AttendanceReportDto>>.Success(reportData);
    }

    public async Task<ServiceResult<List<EmployeeAttendanceReportDto>>> GetEmployeeAttendanceReport(int? year, int? month, int? employeeId, DateTime? FromDate, DateTime? ToDate)
    {
        var reportData = await _reportRepository.GetEmployeeAttendanceReportAsync(year, month, employeeId, FromDate, ToDate);
        if (reportData == null || !reportData.Any())
            return ServiceResult<List<EmployeeAttendanceReportDto>>.Fail("No attendance data found for the specified criteria.");

        return ServiceResult<List<EmployeeAttendanceReportDto>>.Success(reportData);
    }
    public async Task<ServiceResult<List<LeaveSummaryReportDto>>> GetLeaveSummaryReport(int year, int? month, int? departmentId, DateTime? FromDate, DateTime? ToDate)
    {
        var reportData = await _reportRepository.GetLeaveSummaryReportAsync(year, month,  FromDate, ToDate, departmentId);
        if (reportData == null || !reportData.Any())
            return ServiceResult<List<LeaveSummaryReportDto>>.Fail("No leave summary data found for the specified criteria.");

        return ServiceResult<List<LeaveSummaryReportDto>>.Success(reportData);
    }
}
