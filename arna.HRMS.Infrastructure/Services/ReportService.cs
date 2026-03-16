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

    public async Task<ServiceResult<List<AttendanceReportDto>>> GetAttendanceReport(int? year, int? month, int? employeeId, AttendanceStatus? statusId, DeviceType? device)
    {
        var reportData = await _reportRepository.GetAttendanceReportAsync(year, month, employeeId, statusId, device);
        if (reportData == null || !reportData.Any())
            return ServiceResult<List<AttendanceReportDto>>.Fail("No attendance data found for the specified criteria.");

        return ServiceResult<List<AttendanceReportDto>>.Success(reportData);
    }
}
