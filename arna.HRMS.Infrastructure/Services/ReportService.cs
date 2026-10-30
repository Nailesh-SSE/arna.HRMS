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
        return ServiceResult<List<AttendanceReportDto>>.Success(reportData ?? new List<AttendanceReportDto>());
    }

    public async Task<ServiceResult<List<EmployeeAttendanceReportDto>>> GetEmployeeAttendanceReport(int? year, int? month, int? employeeId, DateTime? FromDate, DateTime? ToDate)
    {
        var reportData = await _reportRepository.GetEmployeeAttendanceReportAsync(year, month, employeeId, FromDate, ToDate);
        return ServiceResult<List<EmployeeAttendanceReportDto>>.Success(reportData ?? new List<EmployeeAttendanceReportDto>());
    }
    public async Task<ServiceResult<List<LeaveSummaryReportDto>>> GetLeaveSummaryReport(int? year, int? month, int? departmentId, DateTime? FromDate, DateTime? ToDate)
    {
        var reportData = await _reportRepository.GetLeaveSummaryReportAsync(year, month, FromDate, ToDate, departmentId);
        return ServiceResult<List<LeaveSummaryReportDto>>.Success(reportData ?? new List<LeaveSummaryReportDto>());
    }
    public async Task<ServiceResult<List<EmployeeLeaveDetailsReportDto>>> GetEmployeeLeaveDetailsReport(int? year, int? month, DateTime? FromDate, DateTime? ToDate, int? employeeId, string? employeeNumber)
    {
        var reportData = await _reportRepository.GetEmployeeLeaveDetailsReportAsync(year, month, FromDate, ToDate, employeeId, employeeNumber);
        return ServiceResult<List<EmployeeLeaveDetailsReportDto>>.Success(reportData ?? new List<EmployeeLeaveDetailsReportDto>());
    }
}
