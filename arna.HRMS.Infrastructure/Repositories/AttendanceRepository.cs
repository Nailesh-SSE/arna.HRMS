using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories.Common.Interfaces;
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

    public async Task<List<Attendance>> GetEmployeeAttendanceByStatus(AttendanceStatus? status, int? empId )
    {
        if (status.HasValue && empId == null)
        {
            var statusId = status.Value;
            return await _baseRepository.Query()
                .Include(x => x.Employee)
                .Where(x => x.IsActive && !x.IsDeleted && x.StatusId == statusId && (!empId.HasValue || x.EmployeeId == empId.Value))
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }
        else if (empId.HasValue && status == null){
            return await _baseRepository.Query()
                .Include(x => x.Employee)
                .Where(x => x.IsActive && !x.IsDeleted && x.EmployeeId == empId.Value)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }
        else if(status.HasValue && empId.HasValue)
        {
            return await _baseRepository.Query()
                .Include(x => x.Employee)
                .Where(x => x.IsActive && !x.IsDeleted && x.StatusId == status.Value && x.EmployeeId == empId.Value)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }
        else
        {
            return await _baseRepository.Query()
                .Include(x => x.Employee)
                .Where(x => x.IsActive && !x.IsDeleted)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }
            
    }

    public async Task<Attendance?> GetAttendanceByIdAsync(int id)
    {
        return await _baseRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);
    }

    public Task<Attendance> CreateAttendanceAsync(Attendance attendance)
    {
        return _baseRepository.AddAsync(attendance);
    }

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(
    int year,
    int month,
    int? empId,
    DateTime? selectedDate,
    AttendanceStatus? statusId)
    {
        var attendanceQuery = _baseRepository.Query()
            .Include(a => a.Employee)
            .Where(a =>
                a.IsActive &&
                !a.IsDeleted &&
                a.Employee.IsActive &&
                !a.Employee.IsDeleted);

        if (selectedDate.HasValue)
        {
            var date = selectedDate.Value.Date;
            attendanceQuery = attendanceQuery.Where(a => a.Date.Date == date);

            year = date.Year;
            month = date.Month;
        }
        else
        {
            attendanceQuery = attendanceQuery.Where(a =>
                a.Date.Year == year &&
                a.Date.Month == month);
        }

        if (empId.HasValue && empId > 0)
            attendanceQuery = attendanceQuery.Where(a => a.EmployeeId == empId.Value);

        if(statusId.HasValue && statusId > 0)
            attendanceQuery = attendanceQuery.Where(a => a.StatusId == statusId.Value);

        var attendances = await attendanceQuery.ToListAsync();

        var festivalDates = (await _festivalHolidayRepository
                .GetByMonthAsync(year, month))
            .Select(f => f.Date.Date)
            .ToList();

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var allDates = selectedDate.HasValue
            ? new List<DateTime> { selectedDate.Value.Date }
            : Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(d => startDate.AddDays(d).Date)
                .ToList();

        var employees = await _employeeRepository.GetEmployeesAsync();

        var employeeLookup = employees.ToDictionary(
            e => e.Id,
            e => (e.FullName, e.EmployeeNumber)
        );

        var employeeIds = empId.HasValue && empId > 0
            ? new List<int> { empId.Value }
            : employeeLookup.Keys.ToList();

        var attendanceLookup = attendances
            .GroupBy(a => new { a.Date.Date, a.EmployeeId })
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<MonthlyAttendanceDto>();

        foreach (var date in allDates)
        {
            var dailyDto = new MonthlyAttendanceDto
            {
                Date = DateOnly.FromDateTime(date),
                Day = date.DayOfWeek.ToString()
            };

            bool isFestivalHoliday = festivalDates.Contains(date);

            foreach (var employeeId in employeeIds)
            {
                attendanceLookup.TryGetValue(
                    new { Date = date, EmployeeId = employeeId },
                    out var dailyRecords);

                employeeLookup.TryGetValue(employeeId, out var emp);

                var employeeName = emp.FullName ?? "Unknown";
                var employeeNumber = emp.EmployeeNumber ?? string.Empty;

                if (dailyRecords == null || !dailyRecords.Any())
                {
                    dailyDto.Employees.Add(new EmployeeDailyAttendanceDto
                    {
                        EmployeeId = employeeId,
                        EmployeeName = employeeName,
                        EmployeeNumber = employeeNumber,
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
                    .Sum(x => x.TotalHours.Value.TotalSeconds);

                var totalSeconds =
                    (clockIn.HasValue && clockOut.HasValue)
                        ? (clockOut.Value - clockIn.Value).TotalSeconds
                        : 0;

                var breakSeconds = Math.Max(0, totalSeconds - workingSeconds);

                var statuses = dailyRecords
                    .Select(x => x.StatusId)
                    .Distinct()
                    .ToList();

                var status = CalculateStatus(
                    statuses,
                    isFestivalHoliday,
                    DateOnly.FromDateTime(date));

                dailyDto.Employees.Add(new EmployeeDailyAttendanceDto
                {
                    EmployeeId = employeeId,
                    EmployeeName = employeeName,
                    EmployeeNumber = employeeNumber,
                    ClockIn = clockIn?.TimeOfDay,
                    ClockOut = clockOut?.TimeOfDay,
                    WorkingHours = TimeSpan.FromSeconds(workingSeconds),
                    BreakDuration = TimeSpan.FromSeconds(breakSeconds),
                    TotalHours = TimeSpan.FromSeconds(totalSeconds),
                    Status = status
                });
            }

            result.Add(dailyDto);
        }

        return result;
    }

    private static string CalculateStatus(
    IEnumerable<AttendanceStatus> statuses,
    bool isFestivalHoliday,
    DateOnly date)
    {
        var hasAttendance = statuses.Any();

        if (isFestivalHoliday)
            return "Holiday";

        if (IsWeekend(date) && !hasAttendance)
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

       /* if (!hasAttendance)
            return "Absent";*/

        return string.Empty;
    }

    private static bool IsWeekend(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday ||
               date.DayOfWeek == DayOfWeek.Sunday;
    }


    public async Task<DateTime?> GetLastAttendanceDateAsync(int employeeId)
    {
        return await _baseRepository.Query()
            .Where(x => x.EmployeeId == employeeId && x.StatusId == AttendanceStatus.Present)
            .OrderByDescending(x => x.Date)
            .Select(x => (DateTime?)x.Date.Date)
            .FirstOrDefaultAsync();
    }

    public async Task<Attendance?> GetLastAttendanceTodayAsync(int employeeId)
    {
        return await _baseRepository.Query()
            .Where(a =>
                a.EmployeeId == employeeId)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();
    }
}
