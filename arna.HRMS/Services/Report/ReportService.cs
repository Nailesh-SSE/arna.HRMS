using arna.HRMS.Models.Common.Result;
using arna.HRMS.Models.Enums;
using arna.HRMS.Models.ViewModels;
using arna.HRMS.Services.Http;

namespace arna.HRMS.Services.Report;

public interface IReportService
{
    Task<ApiResult<List<AttendanceReportViewModel>>> GetAttendanceReportAsync(int? year, int? month, int? employeeId, AttendanceStatus? StatusId, DeviceType? device);
}

public class ReportService : IReportService
{
    private readonly ApiClients.ReportApi _report;

    public ReportService(ApiClients api)
    {
        _report = api.Report;
    }

    public async Task<ApiResult<List<AttendanceReportViewModel>>> GetAttendanceReportAsync(int? year, int? month, int? employeeId, AttendanceStatus? statusId, DeviceType? device)
    {
        return await _report.GetEmployeesAttendanceReportAsync(year, month, employeeId, statusId, device);
    }
}
