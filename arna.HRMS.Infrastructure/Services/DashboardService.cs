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
                    .ToList()
            };
        }

        dashboard.TotalEmployees = dashboard.Employees.Count;
        dashboard.TotalRequests = dashboard.Requests.Count;
        dashboard.PendingRequests = dashboard.Requests.Count(x => x.StatusId == Status.Pending);
        dashboard.ApprovedRequests = dashboard.Requests.Count(x => x.StatusId == Status.Approved);
        dashboard.RejectedRequests = dashboard.Requests.Count(x => x.StatusId == Status.Rejected);
        dashboard.CancledRequests = dashboard.Requests.Count(x => x.StatusId == Status.Cancelled);

        return ServiceResult<DashboardDto>.Success(dashboard);
    }
}
