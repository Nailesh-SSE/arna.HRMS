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
            .OrderByDescending(l => l.StatusId == Status.Pending)
            .ThenByDescending(l => l.Id)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<AttendanceRequest>> GetAllRecentAttendanceRequestsAsync(int count = 50)
    {
        return await _attendanceRequestRepository.Query()
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => !a.IsDeleted)
            .OrderByDescending(a => a.StatusId == Status.Pending)
            .ThenByDescending(a => a.Id)
            .Take(count)
            .ToListAsync();
    }
    
    public async Task<List<EmployeeDailyAttendanceDto>> GetTodayPresentEmployeesAsync()
    {
        var today = DateTime.Today;

        var attendances = await _attendanceRepository.Query()
            .AsNoTracking()
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
            .GroupBy(a => new { a.EmployeeId, a.Employee.FullName, a.Employee.EmployeeNumber, a.Employee.ProfileImage })
            .Select(group => MapPresentEmployeeDailyAttendance(
                today,
                group.Key.EmployeeId,
                group.Key.FullName ?? "Unknown",
                group.Key.EmployeeNumber ?? string.Empty,
                group.Key.ProfileImage,
                group.ToList()))
            .ToList();

        return result;
    }

    private static EmployeeDailyAttendanceDto MapPresentEmployeeDailyAttendance(
        DateTime date,
        int employeeId,
        string employeeName,
        string employeeNumber,
        string? profileImage,
        List<Attendance> records)
    {
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

        var breaks = ComputeBreaks(clockIns, clockOuts);

        var workingSeconds = records
            .Where(x => x.TotalHours.HasValue)
            .Sum(x => x.TotalHours!.Value.TotalSeconds);

        var breakSeconds = breaks.Sum(b => b.Duration.TotalSeconds);
        var totalSeconds = workingSeconds + breakSeconds;

        var firstClockIn = clockIns.Any() ? clockIns.First().TimeOfDay : (TimeSpan?)null;
        var lastClockOut = clockOuts.Any() ? clockOuts.Last().TimeOfDay : (TimeSpan?)null;

        return new EmployeeDailyAttendanceDto
        {
            Date = date,
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            EmployeeNumber = employeeNumber,
            ProfileImage = profileImage,
            ClockIn = firstClockIn,
            ClockOut = lastClockOut,
            WorkingHours = TimeSpan.FromSeconds(workingSeconds > 0 ? workingSeconds : 8 * 3600),
            TotalHours = TimeSpan.FromSeconds(totalSeconds > 0 ? totalSeconds : 8 * 3600),
            Breaks = breaks,
            Status = "Present"
        };
    }

    private static List<BreakDto> ComputeBreaks(List<DateTime> clockIns, List<DateTime> clockOuts)
    {
        return clockOuts
            .Zip(clockIns.Skip(1), (sessionEnd, nextStart) => new { sessionEnd, nextStart })
            .Where(pair => pair.nextStart > pair.sessionEnd)
            .Select(pair => new BreakDto
            {
                BreakStart = pair.sessionEnd.TimeOfDay,
                BreakEnd = pair.nextStart.TimeOfDay,
                Duration = pair.nextStart - pair.sessionEnd
            })
            .ToList();
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

    public async Task<List<EmployeeDailyAttendanceDto>> GetTodayLeaveEmployeesAsync(
        HashSet<int>? presentEmpIds = null,
        IReadOnlyList<Employee>? allEmployees = null)
    {
        var today = DateTime.Today;

        var approvedLeavesToday = await GetApprovedLeavesTodayAsync(today);
        var leaveEmpIds = approvedLeavesToday.Select(x => x.EmployeeId).ToHashSet();

        var attendanceLeaves = await GetAttendanceLeavesTodayAsync(today, leaveEmpIds);
        var allLeaves = approvedLeavesToday.Concat(attendanceLeaves).ToList();
        var allLeaveEmpIds = allLeaves.Select(x => x.EmployeeId).ToHashSet();

        var presents = presentEmpIds ?? await GetTodayPresentEmployeeIdsAsync(today);

        List<EmployeeDailyAttendanceDto> absentEmployees;
        if (allEmployees != null)
        {
            absentEmployees = allEmployees
                .Where(e => e.IsActive && !e.IsDeleted && !presents.Contains(e.Id) && !allLeaveEmpIds.Contains(e.Id))
                .Select(emp => new EmployeeDailyAttendanceDto
                {
                    Date = today,
                    EmployeeId = emp.Id,
                    EmployeeNumber = emp.EmployeeNumber,
                    EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                    ProfileImage = emp.ProfileImage,
                    ClockIn = null,
                    ClockOut = null,
                    WorkingHours = TimeSpan.Zero,
                    TotalHours = TimeSpan.Zero,
                    Status = "Absent",
                    Note = "Not Clocked In"
                })
                .ToList();
        }
        else
        {
            absentEmployees = await GetAbsentEmployeesTodayAsync(today, presents, allLeaveEmpIds);
        }

        var result = allLeaves.Concat(absentEmployees).ToList();
        return result;
    }

    private async Task<List<EmployeeDailyAttendanceDto>> GetApprovedLeavesTodayAsync(DateTime today)
    {
        return await _leaveRequestRepository.Query()
            .AsNoTracking()
            .Include(l => l.Employee)
            .Include(l => l.LeaveType)
            .Where(l => !l.IsDeleted && l.StatusId == Status.Approved && l.StartDate.Date <= today && l.EndDate.Date >= today)
            .Select(l => new EmployeeDailyAttendanceDto
            {
                Date = today,
                EmployeeId = l.EmployeeId,
                EmployeeNumber = l.Employee != null ? l.Employee.EmployeeNumber : $"EMP{l.EmployeeId:D3}",
                EmployeeName = l.Employee != null ? $"{l.Employee.FirstName} {l.Employee.LastName}".Trim() : "Unknown",
                ProfileImage = l.Employee != null ? l.Employee.ProfileImage : null,
                ClockIn = null,
                ClockOut = null,
                WorkingHours = TimeSpan.Zero,
                TotalHours = TimeSpan.Zero,
                Status = "On Leave",
                Note = l.LeaveType != null ? ((LeaveName)l.LeaveType.LeaveNameId).ToString() : (string.IsNullOrWhiteSpace(l.Reason) ? "On Leave" : l.Reason)
            })
            .ToListAsync();
    }

    private async Task<List<EmployeeDailyAttendanceDto>> GetAttendanceLeavesTodayAsync(DateTime today, HashSet<int> excludeEmpIds)
    {
        return await _attendanceRepository.Query()
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => a.IsActive && !a.IsDeleted && a.Date == today && a.StatusId == AttendanceStatus.Leave && !excludeEmpIds.Contains(a.EmployeeId))
            .Select(att => new EmployeeDailyAttendanceDto
            {
                Date = att.Date,
                EmployeeId = att.EmployeeId,
                EmployeeNumber = att.Employee != null ? att.Employee.EmployeeNumber : $"EMP{att.EmployeeId:D3}",
                EmployeeName = att.Employee != null ? $"{att.Employee.FirstName} {att.Employee.LastName}".Trim() : "Unknown",
                ProfileImage = att.Employee != null ? att.Employee.ProfileImage : null,
                ClockIn = null,
                ClockOut = null,
                WorkingHours = TimeSpan.Zero,
                TotalHours = TimeSpan.Zero,
                Status = "On Leave",
                Note = string.IsNullOrWhiteSpace(att.Notes) ? "Approved Leave" : att.Notes
            })
            .ToListAsync();
    }

    private async Task<HashSet<int>> GetTodayPresentEmployeeIdsAsync(DateTime today)
    {
        var ids = await _attendanceRepository.Query()
            .AsNoTracking()
            .Where(a => a.IsActive && !a.IsDeleted && a.Date == today &&
                        (a.ClockIn != null || a.ClockOut != null || a.StatusId == AttendanceStatus.Present))
            .Select(a => a.EmployeeId)
            .Distinct()
            .ToListAsync();

        return ids.ToHashSet();
    }

    private async Task<List<EmployeeDailyAttendanceDto>> GetAbsentEmployeesTodayAsync(DateTime today, HashSet<int> presentEmpIds, HashSet<int> allLeaveEmpIds)
    {
        return await _employeeRepository.Query()
            .AsNoTracking()
            .Where(e => e.IsActive && !e.IsDeleted && !presentEmpIds.Contains(e.Id) && !allLeaveEmpIds.Contains(e.Id))
            .Select(emp => new EmployeeDailyAttendanceDto
            {
                Date = today,
                EmployeeId = emp.Id,
                EmployeeNumber = emp.EmployeeNumber,
                EmployeeName = $"{emp.FirstName} {emp.LastName}".Trim(),
                ProfileImage = emp.ProfileImage,
                ClockIn = null,
                ClockOut = null,
                WorkingHours = TimeSpan.Zero,
                TotalHours = TimeSpan.Zero,
                Status = "Absent",
                Note = "Not Clocked In"
            })
            .ToListAsync();
    }

    public async Task<MonthlyAttendanceStatsDto> GetCurrentMonthAttendanceStatsAsync()
    {
        var now = DateTime.Today;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1);

        var statsCounts = await _attendanceRepository.Query()
            .AsNoTracking()
            .Where(a => a.IsActive && !a.IsDeleted && a.Date >= startOfMonth && a.Date < endOfMonth)
            .GroupBy(a => a.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync();

        var stats = new MonthlyAttendanceStatsDto
        {
            Present = statsCounts.FirstOrDefault(x => x.StatusId == AttendanceStatus.Present)?.Count ?? 0,
            Leave = statsCounts.FirstOrDefault(x => x.StatusId == AttendanceStatus.Leave)?.Count ?? 0,
            Absent = statsCounts.FirstOrDefault(x => x.StatusId == AttendanceStatus.Absent)?.Count ?? 0,
            HalfDay = statsCounts.FirstOrDefault(x => x.StatusId == AttendanceStatus.HalfDay)?.Count ?? 0
        };

        return stats;
    }
}
