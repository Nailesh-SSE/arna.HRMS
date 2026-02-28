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

    public async Task<List<Attendance>> GetEmployeeAttendanceByStatus(AttendanceStatus? status, int? empId)
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

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int? empId, DateTime? selectedDate, AttendanceStatus? statusId)
    {
        var query = _baseRepository.Query()
            .Where(a => a.IsActive && !a.IsDeleted && a.Employee.IsActive && !a.Employee.IsDeleted);

        DateTime startDate;
        DateTime endDate;

        if (selectedDate.HasValue)
        {
            var date = selectedDate.Value.Date;
            startDate = date;
            endDate = date;

            year = date.Year;
            month = date.Month;
        }
        else
        {
            startDate = new DateTime(year, month, 1);
            endDate = startDate.AddMonths(1).AddDays(-1);
        }

        query = query.Where(a =>
            a.Date >= startDate && a.Date <= endDate);

        if (empId.HasValue && empId > 0)
            query = query.Where(a => a.EmployeeId == empId.Value);

        if (statusId.HasValue)
            query = query.Where(a => a.StatusId == statusId.Value);

        var attendances = await query.ToListAsync();

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

        var employeeIds = employeeLookup.Keys.ToList();

        // Festival dates (HashSet for O(1) lookup)
        var festivalDates = (await _festivalHolidayRepository
                .GetByMonthAsync(year, month))
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

        var result = new List<MonthlyAttendanceDto>();

        foreach (var date in allDates)
        {
            var dailyDto = new MonthlyAttendanceDto
            {
                Date = DateOnly.FromDateTime(date),
                Day = date.DayOfWeek.ToString()
            };

            bool isFestivalHoliday = festivalDates.Contains(date.Date);

            foreach (var employeeId in employeeIds)
            {
                attendanceLookup.TryGetValue(
                    new { Date = date.Date, EmployeeId = employeeId },
                    out var dailyRecords);

                var emp = employeeLookup[employeeId];

                if (dailyRecords == null || !dailyRecords.Any())
                {
                    dailyDto.Employees.Add(new EmployeeDailyAttendanceDto
                    {
                        EmployeeId = employeeId,
                        EmployeeName = emp.FullName ?? "Unknown",
                        EmployeeNumber = emp.EmployeeNumber ?? string.Empty,
                        ClockIn = null,
                        ClockOut = null,
                        WorkingHours = TimeSpan.Zero,
                        BreakDuration = TimeSpan.Zero,
                        TotalHours = TimeSpan.Zero,
                        Status = CalculateStatus(
                            Enumerable.Empty<AttendanceStatus>(),
                            isFestivalHoliday,
                            DateOnly.FromDateTime(date))
                    });

                    continue;
                }

                var clockIn = dailyRecords
                    .Where(x => x.ClockIn.HasValue)
                    .Min(x => x.ClockIn);

                var clockOut = dailyRecords
                    .Where(x => x.ClockOut.HasValue)
                    .Max(x => x.ClockOut);

                var workingSeconds = dailyRecords
                    .Where(x => x.TotalHours.HasValue)
                     .Sum(x => x.TotalHours?.TotalSeconds ?? 0);

                var totalSeconds =
                    (clockIn.HasValue && clockOut.HasValue)
                        ? (clockOut.Value - clockIn.Value).TotalSeconds
                        : 0;

                var breakSeconds = Math.Max(0, totalSeconds - workingSeconds);

                var statuses = dailyRecords
                    .Select(x => x.StatusId)
                    .Distinct();

                dailyDto.Employees.Add(new EmployeeDailyAttendanceDto
                {
                    EmployeeId = employeeId,
                    EmployeeName = emp.FullName ?? "Unknown",
                    EmployeeNumber = emp.EmployeeNumber ?? string.Empty,
                    ClockIn = clockIn?.TimeOfDay,
                    ClockOut = clockOut?.TimeOfDay,
                    WorkingHours = TimeSpan.FromSeconds(workingSeconds),
                    BreakDuration = TimeSpan.FromSeconds(breakSeconds),
                    TotalHours = TimeSpan.FromSeconds(totalSeconds),
                    Status = CalculateStatus(
                        statuses,
                        isFestivalHoliday,
                        DateOnly.FromDateTime(date))
                });
            }

            result.Add(dailyDto);
        }

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
                a.EmployeeId == employeeId)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();
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

        if (date.DayOfWeek == DayOfWeek.Saturday ||
            date.DayOfWeek == DayOfWeek.Sunday && !hasAttendance)
            return "Holiday";

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
