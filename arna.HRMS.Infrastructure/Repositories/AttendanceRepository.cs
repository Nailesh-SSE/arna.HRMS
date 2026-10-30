using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Repository;
using Microsoft.EntityFrameworkCore;

namespace arna.HRMS.Infrastructure.Repositories;

public class AttendanceRepository
{
    private readonly IBaseRepository<Attendance> _baseRepository;
    private readonly FestivalHolidayRepository _festivalHolidayRepository;
    private readonly EmployeeRepository _employeeRepository;

    public AttendanceRepository(
        IBaseRepository<Attendance> baseRepository,
        EmployeeRepository employeeRepository,
        FestivalHolidayRepository festivalHolidayRepository)
    {
        _baseRepository = baseRepository;
        _employeeRepository = employeeRepository;
        _festivalHolidayRepository = festivalHolidayRepository;
    }

    #region Basic Queries

    public async Task<List<Attendance>> GetAttendanceByStatusAndEmployeeId(AttendanceStatus? status, int? empId)
    {
        // Perf 4: Default to last 6 months to avoid loading entire history
        var sixMonthsAgo = DateTime.Today.AddMonths(-6);

        var query = _baseRepository.Query()
            .AsNoTracking()
            .Include(x => x.Employee)
            .Where(x => !x.IsDeleted && x.Date >= sixMonthsAgo);

        if (status.HasValue)
            query = query.Where(x => x.StatusId == status.Value); 

        if (empId.HasValue)
            query = query.Where(x => x.EmployeeId == empId.Value);

        return await query
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<Attendance?> GetAttendanceByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .AsNoTracking()
            .Where(x => !x.IsDeleted && x.Id == id)
            .FirstOrDefaultAsync();
    }

    public Task<Attendance> CreateAttendanceAsync(Attendance attendance)
    {
        return _baseRepository.AddAsync(attendance);
    }

    #endregion

    #region Monthly Attendance

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(
        int? year,
        int? month,
        int? empId,
        DateTime? selectedDate,
        AttendanceStatus? statusId)
    {
        // Default fallback for null year/month
        var currentDate = selectedDate ?? DateTime.Now;
        var finalYear = selectedDate?.Year ?? year ?? currentDate.Year;
        var finalMonth = selectedDate?.Month ?? month ?? currentDate.Month;

        var startDate = selectedDate?.Date ?? new DateTime(finalYear, finalMonth, 1);
        var endDate = selectedDate?.Date ?? startDate.AddMonths(1).AddDays(-1);

        // Perf 2: Include Employee in the query to avoid separate N+1 fetch
        var baseQuery = _baseRepository.Query()
            .AsNoTracking()
            .Include(a => a.Employee)
            .Where(a => !a.IsDeleted &&
                        !a.Employee.IsDeleted &&
                        a.Date >= startDate && a.Date <= endDate);

        if (empId.HasValue && empId > 0)
            baseQuery = baseQuery.Where(a => a.EmployeeId == empId.Value);

        if (statusId.HasValue)
            baseQuery = baseQuery.Where(a => a.StatusId == statusId.Value);

        var attendances = await baseQuery.ToListAsync();

        List<Employee> employees;

        if (empId.HasValue && empId > 0)
        {
            var employee = await _employeeRepository.GetEmployeeByIdAsync(empId.Value);

            employees = employee != null
                ? new List<Employee> { employee }
                : new List<Employee>();
        }
        else
        {
            employees = await _employeeRepository.GetEmployeesAsync();
        }

        var employeeLookup = employees.ToDictionary(
            e => e.Id,
            e => new { e.FullName, e.EmployeeNumber });

        var festivalDates = (await _festivalHolidayRepository
                .GetByMonthAsync(finalYear, finalMonth))
            .Select(f => f.Date.Date)
            .ToHashSet();

        var allDates = selectedDate.HasValue
            ? new List<DateTime> { startDate }
            : Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(d => startDate.AddDays(d))
                .ToList();

        var attendanceLookup = attendances
            .GroupBy(a => new { Date = a.Date.Date, a.EmployeeId })
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = allDates
            .Select(date =>
            {
                var isFestivalHoliday = festivalDates.Contains(date.Date);

                var employeesData = employeeLookup.Keys
                    .Select(employeeId =>
                    {
                        attendanceLookup.TryGetValue(
                            new { Date = date.Date, EmployeeId = employeeId },
                            out var records);

                        var emp = employeeLookup[employeeId];

                        if (records == null || !records.Any())
                        {
                            return new EmployeeDailyAttendanceDto
                            {
                                Date = date,
                                EmployeeId = employeeId,
                                EmployeeName = emp.FullName ?? "Unknown",
                                EmployeeNumber = emp.EmployeeNumber ?? string.Empty,
                                ClockIn = null,
                                ClockOut = null,
                                WorkingHours = TimeSpan.Zero,
                                Breaks = new List<BreakDto>(),
                                TotalHours = TimeSpan.Zero,
                                Status = CalculateStatus(
                                    Enumerable.Empty<AttendanceStatus>(),
                                    isFestivalHoliday,
                                    DateOnly.FromDateTime(date))
                            };
                        }

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

                        var workingSeconds = records
                            .Where(x => x.TotalHours.HasValue)
                            .Sum(x => x.TotalHours!.Value.TotalSeconds);

                        var breakSeconds = breaks.Sum(b => b.Duration.TotalSeconds);
                        var totalSeconds = workingSeconds + breakSeconds;

                        var firstClockIn = clockIns.Any() ? clockIns.First().TimeOfDay : (TimeSpan?)null;
                        var lastClockOut = clockOuts.Any() ? clockOuts.Last().TimeOfDay : (TimeSpan?)null;

                        var statuses = records.Select(x => x.StatusId).Distinct();

                        var note = records.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.Notes))?.Notes;

                        return new EmployeeDailyAttendanceDto
                        {
                            Date = date,
                            EmployeeId = employeeId,
                            EmployeeName = emp.FullName ?? "Unknown",
                            EmployeeNumber = emp.EmployeeNumber ?? string.Empty,
                            ClockIn = firstClockIn,
                            ClockOut = lastClockOut,
                            WorkingHours = TimeSpan.FromSeconds(workingSeconds),
                            TotalHours = TimeSpan.FromSeconds(totalSeconds),
                            Breaks = breaks,   // ← THIS is what was missing
                            Note = note,
                            Status = CalculateStatus(
                                                 statuses,
                                                 isFestivalHoliday,
                                                 DateOnly.FromDateTime(date))
                        };
                    })
                    .ToList();

                return new MonthlyAttendanceDto
                {
                    Date = DateOnly.FromDateTime(date),
                    Day = date.DayOfWeek.ToString(),
                    Employees = employeesData
                };
            })
            .ToList();

        return result;
    }

    #endregion

    public async Task<DateTime?> GetLastAttendanceDateAsync(int employeeId)
    {
        return await _baseRepository.Query()
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                !x.IsDeleted &&
                x.EmployeeId == employeeId &&
                x.StatusId == AttendanceStatus.Present)
            .OrderByDescending(x => x.Date)
            .Select(x => (DateTime?)x.Date)
            .FirstOrDefaultAsync();
    }

    // Bug 3 FIX: Added a.Date.Date == today filter — previously returned stale data from any day
    public async Task<Attendance?> GetLastAttendanceTodayAsync(int employeeId)
    {
        var today = DateTime.Today;

        return await _baseRepository.Query()
            .AsNoTracking()
            .Where(a =>
                a.IsActive &&
                !a.IsDeleted &&
                a.EmployeeId == employeeId &&
                a.Date.Date == today &&
                (
                    a.StatusId == AttendanceStatus.Present ||
                    a.StatusId == AttendanceStatus.Late ||
                    a.StatusId == AttendanceStatus.HalfDay
                ) &&
                (a.ClockIn != null || a.ClockOut != null)
            )
            .OrderByDescending(a => a.CreatedOn)
            .FirstOrDefaultAsync();
    }

    public async Task<List<Attendance>> GetTodayFirstClockInAsync(int empId)
    {
        var today = DateTime.Today;

        return await _baseRepository.Query()
            .AsNoTracking()
            .Where(a =>
                a.IsActive &&
                !a.IsDeleted &&
                a.EmployeeId == empId &&
                a.Date.Date == today &&
                (
                    a.StatusId == AttendanceStatus.Present ||
                    a.StatusId == AttendanceStatus.Late ||
                    a.StatusId == AttendanceStatus.HalfDay
                ))
            .OrderBy(a => a.CreatedOn)
            .ToListAsync();
    }

    public async Task<bool> DeleteAttendanceAsync(DateTime date, int empId, AttendanceStatus? statusId = null)
    {
        var targetDate = date.Date;
        var query = _baseRepository.Query()
            .Where(ar =>
                ar.Date.Date == targetDate &&
                ar.EmployeeId == empId &&
                !ar.IsDeleted);

        if (statusId.HasValue)
        {
            query = query.Where(ar => ar.StatusId == statusId.Value);
        }

        var entities = await query.ToListAsync();

        if (!entities.Any())
            return false;

        foreach (var entity in entities)
        {
            entity.IsActive = false;
            entity.IsDeleted = true;
            entity.UpdatedOn = DateTime.Now;
            await _baseRepository.UpdateAsync(entity);
        }

        return true;
    }

    public async Task EnsureLeaveAttendanceAsync(int employeeId, DateTime date, string reason)
    {
        var targetDate = date.Date;
        var existing = await _baseRepository.Query()
            .FirstOrDefaultAsync(a =>
                a.EmployeeId == employeeId &&
                a.Date.Date == targetDate &&
                !a.IsDeleted);

        if (existing == null)
        {
            await _baseRepository.AddAsync(new Attendance
            {
                EmployeeId = employeeId,
                Date = targetDate,
                StatusId = AttendanceStatus.Leave,
                TotalHours = TimeSpan.Zero,
                Notes = string.IsNullOrWhiteSpace(reason) ? "Approved Leave" : reason,
                IsActive = true,
                IsDeleted = false,
                DeviceId = DeviceType.LaptopDesktop
            });
        }
    }

    #region Helpers

    // Bug 7 FIX: Check actual attendance records BEFORE holiday/weeklyoff status
    // so that employees who worked on a holiday show as "Present" not "Holiday"
    private static string CalculateStatus(
        IEnumerable<AttendanceStatus> statuses,
        bool isFestivalHoliday,
        DateOnly date)
    {
        // Materialize once to avoid multiple enumerations
        var statusList = statuses as IList<AttendanceStatus> ?? statuses.ToList();
        var hasAttendance = statusList.Count > 0;

        // Check for actual work records first — if they clocked in, they're present
        if (statusList.Contains(AttendanceStatus.Present))
            return "Present";

        if (statusList.Contains(AttendanceStatus.Late))
            return "Late";

        if (statusList.Contains(AttendanceStatus.HalfDay))
            return "HalfDay";

        // Only show Holiday/WeeklyOff if no actual work was done
        if (isFestivalHoliday && !hasAttendance)
            return "Holiday";

        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday && !hasAttendance)
            return "WeeklyOff";

        if (isFestivalHoliday)
            return "Holiday";

        if (statusList.Contains(AttendanceStatus.Leave))
            return "Leave";

        if (statusList.Contains(AttendanceStatus.Absent))
            return "Absent";

        return string.Empty;
    }

    #endregion
}
