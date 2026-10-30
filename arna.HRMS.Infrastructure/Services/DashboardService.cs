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
        DashboardDto dashboard;

        if (employeeId.HasValue)
        {
            var employee = await _repository.EmployeeDashboardAsync(status, employeeId);
            if (employee == null)
                return ServiceResult<DashboardDto>.Fail("Employee not found");

            var leaveTypes = await _leaveService.GetEmployeeLeaveTypesAsync();

            dashboard = new DashboardDto
            {
                Employees = new List<EmployeeDto>
                {
                    _mapper.Map<EmployeeDto>(employee)
                },
                Requests = _mapper.Map<List<LeaveRequestDto>>(employee.LeaveRequests),
                AttendanceRequests = _mapper.Map<List<AttendanceRequestDto>>(employee.AttendanceRequest),
                LeaveTypes = leaveTypes.Data ?? new List<LeaveTypeDto>()
            };

            dashboard.TotalEmployees = 1;
            dashboard.LeaveTotalRequests = dashboard.Requests.Count;
            dashboard.LeavePendingRequests = dashboard.Requests.Count(x => x.StatusId == Status.Pending);
            dashboard.LeaveApprovedRequests = dashboard.Requests.Count(x => x.StatusId == Status.Approved);
            dashboard.LeaveRejectedRequests = dashboard.Requests.Count(x => x.StatusId == Status.Rejected);
            dashboard.LeaveCancledRequests = dashboard.Requests.Count(x => x.StatusId == Status.Cancelled);
            dashboard.AttendanceTotalRequests = dashboard.AttendanceRequests.Count;
            dashboard.AttendancePendingRequests = dashboard.AttendanceRequests.Count(x => x.StatusId == Status.Pending);

            return ServiceResult<DashboardDto>.Success(dashboard);
        }

        var employees = await _repository.AdminDashboardAsync();
        var recentLeaves = await _repository.GetAllRecentLeaveRequestsAsync();
        var recentAttReqs = await _repository.GetAllRecentAttendanceRequestsAsync();

        dashboard = new DashboardDto
        {
            Employees = _mapper.Map<List<EmployeeDto>>(employees),
            Requests = _mapper.Map<List<LeaveRequestDto>>(recentLeaves),
            AttendanceRequests = _mapper.Map<List<AttendanceRequestDto>>(recentAttReqs)
        };

        // Sequential execution - EF Core DbContext does not support concurrent operations
        dashboard.Attendance = await _repository.GetTodayPresentEmployeesAsync();
        dashboard.PresentEmployee = dashboard.Attendance.Count;
        dashboard.LeaveEmployees = await _repository.GetTodayLeaveEmployeesAsync();
        dashboard.TotalEmployees = dashboard.Employees.Count;
        dashboard.AttendanceTotalRequests = dashboard.AttendanceRequests.Count;
        dashboard.AttendancePendingRequests = dashboard.AttendanceRequests.Count(x => x.StatusId == Status.Pending);

        var allEmployeeRecords = new List<EmployeeDailyAttendanceDto>();
        if (dashboard.Attendance != null) allEmployeeRecords.AddRange(dashboard.Attendance);
        if (dashboard.LeaveEmployees != null) allEmployeeRecords.AddRange(dashboard.LeaveEmployees);

        if (dashboard.TotalEmployees > 0 && allEmployeeRecords.Any())
        {
            var totalWorkingHours = allEmployeeRecords.Sum(x => x.WorkingHours.TotalHours);
            var totalBreakHours = allEmployeeRecords.Sum(x => x.BreakDuration.TotalHours);

            dashboard.AvgWorkingHours = (int)(totalWorkingHours / dashboard.TotalEmployees);
            dashboard.AvgBreakHours = (int)(totalBreakHours / dashboard.TotalEmployees);
        }
        else
        {
            dashboard.AvgWorkingHours = 0;
            dashboard.AvgBreakHours = 0;
        }

        dashboard.AttendanceTotalRequests = dashboard.AttendanceRequests.Count;
        dashboard.AttendancePendingRequests = dashboard.AttendanceRequests.Count(x => x.StatusId == Status.Pending);
        dashboard.LeaveEmployees = dashboard.LeaveEmployees ?? new List<EmployeeDailyAttendanceDto>();
        dashboard.AbsentEmployee = Math.Max(0, dashboard.TotalEmployees - dashboard.PresentEmployee - dashboard.LeaveEmployees.Count);
        dashboard.LeaveTotalRequests = dashboard.Requests.Count;
        dashboard.LeavePendingRequests = dashboard.Requests.Count(x => x.StatusId == Status.Pending);
        dashboard.LeaveApprovedRequests = dashboard.Requests.Count(x => x.StatusId == Status.Approved);
        dashboard.LeaveRejectedRequests = dashboard.Requests.Count(x => x.StatusId == Status.Rejected);
        dashboard.LeaveCancledRequests = dashboard.Requests.Count(x => x.StatusId == Status.Cancelled);

        var monthStats = await _repository.GetCurrentMonthAttendanceStatsAsync();
        dashboard.MonthPresentEmployee = monthStats.present;
        dashboard.MonthLeaveEmployee = monthStats.leave;
        dashboard.MonthAbsentEmployee = monthStats.absent;
        dashboard.MonthHalfDayEmployee = monthStats.halfDay;

        return ServiceResult<DashboardDto>.Success(dashboard);
    }
}
