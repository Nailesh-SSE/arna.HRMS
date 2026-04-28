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

    public DashboardRepository(
        IBaseRepository<Employee> employeeRepository,
        IBaseRepository<Attendance> attendanceRepository)
    {
        _employeeRepository = employeeRepository;
        _attendanceRepository = attendanceRepository;
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

    
    public async Task<List<EmployeeDailyAttendanceDto>> GetTodayPresentEmployeesAsync()
    {
        var today = DateTime.Now.Date;

        var attendances = await _attendanceRepository.Query()
            .Include(a => a.Employee)
            .Where(a => a.IsActive && !a.IsDeleted &&
                        a.Date == today &&
                        (a.ClockIn != null || a.ClockOut != null))  // only who has clocked in today
            .ToListAsync();

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
                    WorkingHours = TimeSpan.FromSeconds(workingSeconds),
                    TotalHours = TimeSpan.FromSeconds(totalSeconds),
                    Breaks = breaks
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

    public async Task<List<EmployeeDailyAttendanceDto>> GetTodayLeaveEmployeesAsync()
    {
        var today = DateTime.Now.Date;

        var allEmployees = await _employeeRepository.Query()
            .Where(e => e.IsActive && !e.IsDeleted)
            .ToListAsync();

        var todaysAttendances = await _attendanceRepository.Query()
            .Where(a => a.IsActive && !a.IsDeleted && a.Date == today)
            .Include(a => a.Employee)
            .ToListAsync();

        var presentEmployeeIds = todaysAttendances
            .Where(a => a.IsActive && !a.IsDeleted &&
                        (a.ClockIn != null || a.ClockOut != null))
            .Select(a => a.EmployeeId)
            .Distinct()
            .ToHashSet();

        var leaveEmployees = todaysAttendances
            .Where(a => !presentEmployeeIds.Contains(a.EmployeeId))
            .GroupBy(a => a.EmployeeId)
            .Select(g =>
            {
                var att = g.First();
                return new EmployeeDailyAttendanceDto
                {
                    Date = today,
                    EmployeeId = att.EmployeeId,
                    EmployeeNumber = att.Employee?.EmployeeNumber ?? string.Empty,
                    EmployeeName = att.Employee != null
                        ? $"{att.Employee.FirstName} {att.Employee.LastName}".Trim()
                        : "Unknown",
                    ClockIn = att.ClockIn?.TimeOfDay,
                    ClockOut = att.ClockOut?.TimeOfDay,
                    WorkingHours = att.TotalHours ?? TimeSpan.Zero,
                    TotalHours = att.TotalHours ?? TimeSpan.Zero,
                    Status = att.StatusId.ToString(),
                    Note = string.IsNullOrWhiteSpace(att.Notes) ? null : att.Notes
                };
            })
            .ToList();

        var recordedEmployeeIds = todaysAttendances.Select(a => a.EmployeeId).Distinct().ToHashSet();
        var absentEmployees = allEmployees
            .Where(e => !presentEmployeeIds.Contains(e.Id) &&
                        !todaysAttendances.Any(a => a.EmployeeId == e.Id))
            .Select(e => new EmployeeDailyAttendanceDto
            {
                Date = today,
                EmployeeId = e.Id,
                EmployeeNumber = e.EmployeeNumber,
                EmployeeName = $"{e.FirstName} {e.LastName}".Trim(),
                ClockIn = null,
                ClockOut = null,
                WorkingHours = TimeSpan.Zero,
                TotalHours = TimeSpan.Zero,
                Status = "Absent",
                Note = null
            })
            .ToList();

        var result = new List<EmployeeDailyAttendanceDto>(leaveEmployees.Count + absentEmployees.Count);
        result.AddRange(leaveEmployees);
        result.AddRange(absentEmployees);

        return result;
    }
}