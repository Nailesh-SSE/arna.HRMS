using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class DashboardRepository
{
    private readonly IBaseRepository<Employee> _employeeRepository;
    private readonly IBaseRepository<Attendance> _attendanceRepository;
    private readonly IBaseRepository<LeaveRequest> _leaveRequestRepository;
    private readonly IBaseRepository<AttendanceRequest> _attendanceRequestRepository;

    public DashboardRepository(
        IBaseRepository<Employee> employeeRepository,
        IBaseRepository<Attendance> attendanceRepository,
        IBaseRepository<LeaveRequest> leaveRequestRepository,
        IBaseRepository<AttendanceRequest> attendanceRequestRepository)
    {
        _employeeRepository = employeeRepository;
        _attendanceRepository = attendanceRepository;
        _leaveRequestRepository = leaveRequestRepository;
        _attendanceRequestRepository = attendanceRequestRepository;
    }

    public async Task<List<Employee>> AdminDashboardAsync()
    {
        return await _employeeRepository.Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.AttendanceRequest)
            .Include(x => x.LeaveRequests)
                .ThenInclude(x => x.LeaveType)
            .Where(x => x.IsActive && !x.IsDeleted)
            .ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetAllRecentLeaveRequestsAsync(int count = 50)
    {
        return await _leaveRequestRepository.Query()
            .AsNoTracking()
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => !l.IsDeleted)
            .OrderByDescending(l => l.Id)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<AttendanceRequest>> GetAllRecentAttendanceRequestsAsync(int count = 50)
    {
        return await _attendanceRequestRepository.Query()
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.Id)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<List<EmployeeDailyAttendanceDto>> GetTodayPresentEmployeesAsync()
    {
        var today = DateTime.Today;

        var attendances = await _attendanceRepository.Query()
            .Include(a => a.Employee)
            .Where(a => a.IsActive && !a.IsDeleted &&
                        a.Date == today &&
                        (a.ClockIn != null || a.ClockOut != null || a.StatusId == AttendanceStatus.Present))
            .ToListAsync();

        if (!attendances.Any())
        {
            return new List<EmployeeDailyAttendanceDto>();
        }

        var result = attendances
            .GroupBy(a => new { a.EmployeeId, a.Employee.FullName, a.Employee.EmployeeNumber })
            .Select(group =>
            {
                var records = group.ToList();

                // Step 1: Ordered clock-in and clock-out lists
                var clockIns = records
                    .Where(x => x.ClockIn.HasValue)
                    .Select(x => x.ClockIn!.Value)
                    .OrderBy(x => x)
                    .ToList();

                var clockOuts = records
                    .Where(x => x.ClockOut.HasValue)
                    .Select(x => x.ClockOut!.Value)
                    .OrderBy(x => x)
                    .ToList();

                // Step 2: Build breaks list dynamically
                var breaks = clockOuts
                    .Zip(clockIns.Skip(1), (sessionEnd, nextStart) => new { sessionEnd, nextStart })
                    .Where(pair => pair.nextStart > pair.sessionEnd)
                    .Select(pair => new BreakDto
                    {
                        BreakStart = pair.sessionEnd.TimeOfDay,
                        BreakEnd = pair.nextStart.TimeOfDay,
                        Duration = pair.nextStart - pair.sessionEnd
                    })
                    .ToList();

                // Step 3: Calculate hours
                var workingSeconds = records
                    .Where(x => x.TotalHours.HasValue)
                    .Sum(x => x.TotalHours!.Value.TotalSeconds);

                var breakSeconds = breaks.Sum(b => b.Duration.TotalSeconds);
                var totalSeconds = workingSeconds + breakSeconds;

                // Step 4: Earliest clock-in / latest clock-out
                var firstClockIn = clockIns.Any() ? clockIns.First().TimeOfDay : (TimeSpan?)null;
                var lastClockOut = clockOuts.Any() ? clockOuts.Last().TimeOfDay : (TimeSpan?)null;

                return new EmployeeDailyAttendanceDto
                {
                    Date = today,
                    EmployeeId = group.Key.EmployeeId,
                    EmployeeName = group.Key.FullName ?? "Unknown",
                    EmployeeNumber = group.Key.EmployeeNumber ?? string.Empty,
                    ClockIn = firstClockIn,
                    ClockOut = lastClockOut,
                    WorkingHours = TimeSpan.FromSeconds(workingSeconds > 0 ? workingSeconds : 8 * 3600),
                    TotalHours = TimeSpan.FromSeconds(totalSeconds > 0 ? totalSeconds : 8 * 3600),
                    Breaks = breaks,
                    Status = "Present"
                };
            })
            .ToList();

        return result;
    }

    public async Task<Employee?> EmployeeDashboardAsync(Status? status, int? employeeId)
    {
        var query = _employeeRepository.Query()
            .AsNoTracking()
            .Include(x => x.Department)
            .Include(x => x.Manager)
            .Include(x => x.AttendanceRequest)
            .Include(x => x.LeaveRequests)
                .ThenInclude(x => x.LeaveType)
            .Where(x => x.Id == employeeId && x.IsActive && !x.IsDeleted);

        var employee = await query.FirstOrDefaultAsync();

        if (employee != null && status.HasValue)
        {
            employee.LeaveRequests = employee.LeaveRequests
                .Where(x => x.StatusId == status.Value)
                .ToList();

            if (employee.AttendanceRequest != null)
            {
                employee.AttendanceRequest = employee.AttendanceRequest
                    .Where(x => x.StatusId == status.Value)
                    .ToList();
            }
        }

        return employee;
    }

    public async Task<List<EmployeeDailyAttendanceDto>> GetTodayLeaveEmployeesAsync()
    {
        var today = DateTime.Today;

        // 1. Employees with approved leave requests covering today
        var approvedLeavesToday = await _leaveRequestRepository.Query()
            .AsNoTracking()
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => !l.IsDeleted && l.StatusId == Status.Approved && l.StartDate.Date <= today && l.EndDate.Date >= today)
            .ToListAsync();

        var leaveDtos = approvedLeavesToday.Select(l => new EmployeeDailyAttendanceDto
        {
            Date = today,
            EmployeeId = l.EmployeeId,
            EmployeeNumber = l.Employee?.EmployeeNumber ?? $"EMP{l.EmployeeId:D3}",
            EmployeeName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}".Trim() : "Unknown",
            ClockIn = null,
            ClockOut = null,
            WorkingHours = TimeSpan.Zero,
            TotalHours = TimeSpan.Zero,
            Status = "On Leave",
            Note = l.LeaveType != null ? ((LeaveName)l.LeaveType.LeaveNameId).ToString() : (string.IsNullOrWhiteSpace(l.Reason) ? "On Leave" : l.Reason)
        }).ToList();

        // 2. Also check if any attendance today is explicitly recorded with StatusId == AttendanceStatus.Leave
        var attendanceLeaves = await _attendanceRepository.Query()
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.IsActive && !a.IsDeleted && a.Date == today && a.StatusId == AttendanceStatus.Leave)
            .ToListAsync();

        foreach (var att in attendanceLeaves)
        {
            if (!leaveDtos.Any(x => x.EmployeeId == att.EmployeeId))
            {
                leaveDtos.Add(new EmployeeDailyAttendanceDto
                {
                    Date = att.Date,
                    EmployeeId = att.EmployeeId,
                    EmployeeNumber = att.Employee?.EmployeeNumber ?? $"EMP{att.EmployeeId:D3}",
                    EmployeeName = att.Employee != null ? $"{att.Employee.FirstName} {att.Employee.LastName}".Trim() : "Unknown",
                    ClockIn = null,
                    ClockOut = null,
                    WorkingHours = TimeSpan.Zero,
                    TotalHours = TimeSpan.Zero,
                    Status = "On Leave",
                    Note = string.IsNullOrWhiteSpace(att.Notes) ? "Approved Leave" : att.Notes
                });
            }
        }

        // 3. Find employees who clocked in today
        var presentEmployeeIds = await _attendanceRepository.Query()
            .Where(a => a.IsActive && !a.IsDeleted && a.Date == today &&
                        (a.ClockIn != null || a.ClockOut != null || a.StatusId == AttendanceStatus.Present))
            .Select(a => a.EmployeeId)
            .Distinct()
            .ToListAsync();

        var presentSet = presentEmployeeIds.ToHashSet();
        var leaveSet = leaveDtos.Select(x => x.EmployeeId).ToHashSet();

        // 4. All active employees who have NOT clocked in and are NOT on approved leave
        // are marked as Absent (Not Clocked In) for today
        var allEmployees = await _employeeRepository.Query()
            .Where(e => e.IsActive && !e.IsDeleted)
            .ToListAsync();

        foreach (var emp in allEmployees)
        {
            if (!presentSet.Contains(emp.Id) && !leaveSet.Contains(emp.Id))
            {
                leaveSet.Add(emp.Id);
                leaveDtos.Add(new EmployeeDailyAttendanceDto
                {
                    Date = today,
                    EmployeeId = emp.Id,
                    EmployeeNumber = emp.EmployeeNumber ?? $"EMP{emp.Id:D3}",
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    ClockIn = null,
                    ClockOut = null,
                    WorkingHours = TimeSpan.Zero,
                    TotalHours = TimeSpan.Zero,
                    Status = "Absent",
                    Note = "Not Clocked In"
                });
            }
        }

        return leaveDtos;
    }

    public async Task<(int present, int leave, int absent, int halfDay)> GetCurrentMonthAttendanceStatsAsync()
    {
        var now = DateTime.Today;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var monthAttendances = await _attendanceRepository.Query()
            .AsNoTracking()
            .Where(a => a.IsActive && !a.IsDeleted && a.Date >= startOfMonth && a.Date < endOfMonth)
            .ToListAsync();

        int present = monthAttendances.Count(a => a.StatusId == AttendanceStatus.Present);
        int leave = monthAttendances.Count(a => a.StatusId == AttendanceStatus.Leave);
        int absent = monthAttendances.Count(a => a.StatusId == AttendanceStatus.Absent);
        int halfDay = monthAttendances.Count(a => a.StatusId == AttendanceStatus.HalfDay);

        return (present, leave, absent, halfDay);
    }
}