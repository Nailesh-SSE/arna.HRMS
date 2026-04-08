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
            var leaveTypes = await _leaveService.GetEmployeeLeaveTypesAsync();

            if (employee == null)
                return ServiceResult<DashboardDto>.Fail("Employee not found");

            dashboard = new DashboardDto
            {
                Employees = new List<EmployeeDto>
                {
                    _mapper.Map<EmployeeDto>(employee)
                },

                Requests = _mapper.Map<List<LeaveRequestDto>>(employee.LeaveRequests),

                LeaveTypes = leaveTypes.Data!
            };
        }
        else
        {
            var employees = await _repository.AdminDashboardAsync();

            dashboard = new DashboardDto
            {
                Employees = _mapper.Map<List<EmployeeDto>>(employees),
                Requests = employees
                    .SelectMany(x => x.LeaveRequests)
                    .Select(x => _mapper.Map<LeaveRequestDto>(x))
                    .ToList(),
                AttendanceRequests = employees
                    .SelectMany(a => a.AttendanceRequest)
                    .Select(x => _mapper.Map<AttendanceRequestDto>(x))
                    .ToList()

            };
            
        }

        dashboard.Attendance = await _repository.GetTodayPresentEmployeesAsync();
        dashboard.PresentEmployee = dashboard.Attendance.Count;
        dashboard.AttendanceTotalRequests = dashboard.AttendanceRequests.Count;
        dashboard.AttendancePendingRequests = dashboard.AttendanceRequests.Count(x => x.StatusId == Status.Pending);
        dashboard.LeaveEmployees = await _repository.GetTodayLeaveEmployeesAsync();
        dashboard.TotalEmployees = dashboard.Employees.Count;

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
        dashboard.AbsentEmployee = dashboard.TotalEmployees - dashboard.PresentEmployee;
        dashboard.LeaveTotalRequests = dashboard.Requests.Count;
        dashboard.LeavePendingRequests = dashboard.Requests.Count(x => x.StatusId == Status.Pending);
        dashboard.LeaveApprovedRequests = dashboard.Requests.Count(x => x.StatusId == Status.Approved);
        dashboard.LeaveRejectedRequests = dashboard.Requests.Count(x => x.StatusId == Status.Rejected);
        dashboard.LeaveCancledRequests = dashboard.Requests.Count(x => x.StatusId == Status.Cancelled);

        return ServiceResult<DashboardDto>.Success(dashboard);
    }
}
