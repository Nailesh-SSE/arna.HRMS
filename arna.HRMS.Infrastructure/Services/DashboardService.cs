using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
using AutoMapper;
namespace arna.HRMS.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private readonly DashboardRepository _repository;
    private readonly IMapper _mapper;
    private readonly ILeaveService _leaveService;

    public DashboardService(DashboardRepository repository, IMapper mapper, ILeaveService leaveService)
    {
        _repository = repository;
        _mapper = mapper;
        _leaveService = leaveService;
    }
    public async Task<ServiceResult<DashboardDto>> GetDashboardAsync(Status? status, int? employeeId)
    {
        var result = employeeId.HasValue
            ? await BuildEmployeeDashboardAsync(status, employeeId.Value)
            : await BuildAdminDashboardAsync();

        return result;
    }

    private async Task<ServiceResult<DashboardDto>> BuildEmployeeDashboardAsync(Status? status, int employeeId)
    {
        var employee = await _repository.EmployeeDashboardAsync(status, employeeId);
        if (employee == null)
        {
            var notFound = ServiceResult<DashboardDto>.Fail("Employee not found");
            return notFound;
        }

        var leaveTypes = await _leaveService.GetEmployeeLeaveTypesAsync();

        var requests = _mapper.Map<List<LeaveRequestDto>>(employee.LeaveRequests);
        var attendanceRequests = _mapper.Map<List<AttendanceRequestDto>>(employee.AttendanceRequest);

        var dashboard = new DashboardDto
        {
            Employees = new List<EmployeeDto> { _mapper.Map<EmployeeDto>(employee) },
            Requests = requests,
            AttendanceRequests = attendanceRequests,
            LeaveTypes = leaveTypes.Data ?? new List<LeaveTypeDto>(),
            TotalEmployees = 1,
            LeaveTotalRequests = requests.Count,
            LeavePendingRequests = requests.Count(x => x.StatusId == Status.Pending),
            LeaveApprovedRequests = requests.Count(x => x.StatusId == Status.Approved),
            LeaveRejectedRequests = requests.Count(x => x.StatusId == Status.Rejected),
            LeaveCancledRequests = requests.Count(x => x.StatusId == Status.Cancelled),
            AttendanceTotalRequests = attendanceRequests.Count,
            AttendancePendingRequests = attendanceRequests.Count(x => x.StatusId == Status.Pending)
        };

        var response = ServiceResult<DashboardDto>.Success(dashboard);
        return response;
    }

    private async Task<ServiceResult<DashboardDto>> BuildAdminDashboardAsync()
    {
        var employees = await _repository.AdminDashboardAsync();
        var recentLeaves = await _repository.GetAllRecentLeaveRequestsAsync();
        var recentAttReqs = await _repository.GetAllRecentAttendanceRequestsAsync();

        var employeeDtos = _mapper.Map<List<EmployeeDto>>(employees);
        var requests = _mapper.Map<List<LeaveRequestDto>>(recentLeaves);
        var attendanceRequests = _mapper.Map<List<AttendanceRequestDto>>(recentAttReqs);

        var presentList = await _repository.GetTodayPresentEmployeesAsync();
        var presentEmpIds = presentList.Select(p => p.EmployeeId).ToHashSet();
        var leaveList = await _repository.GetTodayLeaveEmployeesAsync(presentEmpIds, employees);
        var monthStats = await _repository.GetCurrentMonthAttendanceStatsAsync();

        var dashboard = new DashboardDto
        {
            Employees = employeeDtos,
            Requests = requests,
            AttendanceRequests = attendanceRequests,
            Attendance = presentList,
            PresentEmployee = presentList.Count,
            LeaveEmployees = leaveList,
            TotalEmployees = employeeDtos.Count,
            AttendanceTotalRequests = attendanceRequests.Count,
            AttendancePendingRequests = attendanceRequests.Count(x => x.StatusId == Status.Pending),
            LeaveTotalRequests = requests.Count,
            LeavePendingRequests = requests.Count(x => x.StatusId == Status.Pending),
            LeaveApprovedRequests = requests.Count(x => x.StatusId == Status.Approved),
            LeaveRejectedRequests = requests.Count(x => x.StatusId == Status.Rejected),
            LeaveCancledRequests = requests.Count(x => x.StatusId == Status.Cancelled),
            AbsentEmployee = Math.Max(0, employeeDtos.Count - presentList.Count - leaveList.Count),
            MonthPresentEmployee = monthStats.Present,
            MonthLeaveEmployee = monthStats.Leave,
            MonthAbsentEmployee = monthStats.Absent,
            MonthHalfDayEmployee = monthStats.HalfDay
        };

        CalculateAverageHours(dashboard, presentList, leaveList);

        var response = ServiceResult<DashboardDto>.Success(dashboard);
        return response;
    }

    private static void CalculateAverageHours(DashboardDto dashboard, List<EmployeeDailyAttendanceDto> present, List<EmployeeDailyAttendanceDto> onLeave)
    {
        var allRecords = new List<EmployeeDailyAttendanceDto>();
        if (present != null) allRecords.AddRange(present);
        if (onLeave != null) allRecords.AddRange(onLeave);

        if (dashboard.TotalEmployees > 0 && allRecords.Any())
        {
            var totalWorkingHours = allRecords.Sum(x => x.WorkingHours.TotalHours);
            var totalBreakHours = allRecords.Sum(x => x.BreakDuration.TotalHours);

            dashboard.AvgWorkingHours = (int)(totalWorkingHours / dashboard.TotalEmployees);
            dashboard.AvgBreakHours = (int)(totalBreakHours / dashboard.TotalEmployees);
        }
        else
        {
            dashboard.AvgWorkingHours = 0;
            dashboard.AvgBreakHours = 0;
        }
    }
}
