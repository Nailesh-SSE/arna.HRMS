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
        var query = _baseRepository.Query()
            .Include(x => x.Employee)
            .Where(x => x.IsActive && !x.IsDeleted);

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
            .Where(x => x.IsActive && !x.IsDeleted && x.Id == id)
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

        var baseQuery = _baseRepository.Query()
            .Where(a => a.IsActive && !a.IsDeleted &&
                        a.Employee.IsActive && !a.Employee.IsDeleted &&
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
            .Where(x =>
                x.IsActive &&
                !x.IsDeleted &&
                x.EmployeeId == employeeId &&
                x.StatusId == AttendanceStatus.Present)
            .OrderByDescending(x => x.Date)
            .Select(x => (DateTime?)x.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<Attendance?> GetLastAttendanceTodayAsync(int employeeId)
    {
        return await _baseRepository.Query()
            .Where(a =>
                a.IsActive &&
                !a.IsDeleted &&
                a.EmployeeId == employeeId &&
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


        #region Helpers

    private static string CalculateStatus(
        IEnumerable<AttendanceStatus> statuses,
        bool isFestivalHoliday,
        DateOnly date)
    {
        var hasAttendance = statuses.Any();

        if (isFestivalHoliday)
            return "Holiday";

        if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday && !hasAttendance)
            return "WeeklyOff";

        if (statuses.Contains(AttendanceStatus.Present))
            return "Present";

        if (statuses.Contains(AttendanceStatus.Late))
            return "Late";

        if (statuses.Contains(AttendanceStatus.HalfDay))
            return "HalfDay";

        if (statuses.Contains(AttendanceStatus.Leave))
            return "Leave";

        if (statuses.Contains(AttendanceStatus.Absent))
            return "Absent";

        return string.Empty;
    }

    #endregion
}
