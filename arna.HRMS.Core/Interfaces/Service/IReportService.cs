using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Interfaces.Service;

public  interface IReportService
{
    Task<ServiceResult<List<AttendanceReportDto>>> GetAttendanceReport(int? year, int? month, int? employeeId, AttendanceStatus? statusId, DeviceType? device);
}
