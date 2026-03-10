using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;

namespace arna.HRMS.Core.Interfaces.Service;

public interface IDashboardService
{
    Task<ServiceResult<DashboardDto>> GetDashboardAsync(Status? status, int? employeeId);
}
