using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class DashboardRepository
{
    private readonly IBaseRepository<Employee> _employeeRepository;
    private readonly IBaseRepository<LeaveType> _leaveTypeRepository;
    private readonly IBaseRepository<LeaveRequest> _leaveRequestRepository;

    public DashboardRepository(
        IBaseRepository<Employee> employeeRepository,
        IBaseRepository<LeaveType> leaveTypeRepository,
        IBaseRepository<LeaveRequest> leaveRequestRepository)
    {
        _employeeRepository = employeeRepository;
        _leaveTypeRepository = leaveTypeRepository;
        _leaveRequestRepository = leaveRequestRepository;
    }

    public async Task<List<Employee>> AdminDashboardAsync()
    {
        return await _employeeRepository.Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.LeaveRequests)
                .ThenInclude(x => x.LeaveType)
            .Where(x => x.IsActive && !x.IsDeleted)
            .ToListAsync();
    }

    public async Task<Employee?> EmployeeDashboardAsync(Status? status, int? employeeId)
    {
        var query = _employeeRepository.Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.LeaveRequests)
                .ThenInclude(x => x.LeaveType)
            .Where(x => x.Id == employeeId && x.IsActive && !x.IsDeleted);

        var employee = await query.FirstOrDefaultAsync();

        if (employee != null && status.HasValue)
        {
            employee.LeaveRequests = employee.LeaveRequests
                .Where(x => x.StatusId == status.Value)
                .ToList();
        }

        return employee;
    }
}