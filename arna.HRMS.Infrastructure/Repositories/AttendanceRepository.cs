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

    public AttendanceRepository(IBaseRepository<Attendance> baseRepository, FestivalHolidayRepository festivalHolidayRepository, EmployeeRepository employeeRepository)
    {
        _baseRepository = baseRepository;
        _festivalHolidayRepository = festivalHolidayRepository;
        _employeeRepository = employeeRepository;
    }

    public async Task<List<Attendance>> GetAttendenceAsync()
    {
        return await _baseRepository.Query()
            .Include(x => x.Employee)
            .Where(x => x.IsActive && !x.IsDeleted)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<Attendance?> GetAttendanceByIdAsync(int id)
    {
        var attendence = await _baseRepository.Query()
            .FirstOrDefaultAsync(x => x.Id == id && x.IsActive && !x.IsDeleted);
        return attendence;
    }

    public Task<Attendance> CreateAttendanceAsync(Attendance attendance)
    {
        return _baseRepository.AddAsync(attendance);
    }

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(
    int year,
    int month,
    int? empId,
    DateTime? selectedDate)
    {
        // 1️⃣ Attendance base query
        var attendanceQuery = _baseRepository.Query()
            .Include(a => a.Employee)
            .Where(a =>
                a.IsActive &&
                !a.IsDeleted &&
                a.Employee.IsActive &&
                !a.Employee.IsDeleted);

        // 📅 Date filter mode
        if (selectedDate.HasValue)
        {
            var date = selectedDate.Value.Date;
            attendanceQuery = attendanceQuery.Where(a => a.Date == date);

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

        var attendances = await attendanceQuery.ToListAsync();

        // 2️⃣ Festival holidays (still monthly scope)
        var festivalDates = (await _festivalHolidayRepository
            .GetByMonthAsync(year, month))
            .Select(f => f.Date.Date)
            .ToHashSet();

        // 3️⃣ Dates to process (single date OR full month)
        List<DateTime> allDates;

        if (selectedDate.HasValue)
        {
            allDates = new List<DateTime> { selectedDate.Value.Date };
        }
        else
        {
            var startDate = new DateTime(year, month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            allDates = Enumerable.Range(0, (endDate - startDate).Days + 1)
                .Select(d => startDate.AddDays(d).Date)
                .ToList();
        }

        // 4️⃣ Employee IDs involved
        var employeeIds = empId.HasValue && empId > 0
            ? new List<int> { empId.Value }
            : attendances.Select(a => a.EmployeeId).Distinct().ToList();

        // 5️⃣ Employee name lookup
        var employeeNameLookup = attendances
            .Where(a => a.Employee != null)
            .GroupBy(a => a.EmployeeId)
            .ToDictionary(
                g => g.Key,
                g => g.First().Employee.FullName
            );

        if (empId.HasValue && empId > 0 && !employeeNameLookup.ContainsKey(empId.Value))
        {
            var emp = await _employeeRepository.GetEmployeeByIdAsync(empId.Value);
            if (emp != null)
                employeeNameLookup[empId.Value] = emp.FullName;
        }

        // 6️⃣ Attendance lookup
        var attendanceLookup = attendances
            .GroupBy(a => new { a.Date.Date, a.EmployeeId })
            .ToDictionary(g => g.Key, g => g.ToList());

        var result = new List<MonthlyAttendanceDto>();

        // 7️⃣ Build response
        foreach (var date in allDates)
        {
            var dailyDto = new MonthlyAttendanceDto
            {
                Date = DateOnly.FromDateTime(date),
                Day = date.DayOfWeek.ToString()
            };

            bool isWeekend =
                date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday;

            bool isFestival = festivalDates.Contains(date);

            foreach (var employeeId in employeeIds)
            {
                attendanceLookup.TryGetValue(
                    new { Date = date, EmployeeId = employeeId },
                    out var dailyRecords);

                var empName = employeeNameLookup.GetValueOrDefault(employeeId, "");

                if (dailyRecords == null || !dailyRecords.Any())
                {
                    dailyDto.Employees.Add(new EmployeeDailyAttendanceDto
                    {
                        EmployeeId = employeeId,
                        EmployeeName = empName,
                        Status = (isWeekend || isFestival) ? "Holiday" : null
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
                    isFestival,
                    DateOnly.FromDateTime(date));

                dailyDto.Employees.Add(new EmployeeDailyAttendanceDto
                {
                    EmployeeId = employeeId,
                    EmployeeName = empName,
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

    private static string CalculateStatus(IEnumerable<AttendanceStatus> statuses, bool isFestivalHoliday, DateOnly date)
    {
        if (isFestivalHoliday)
            return "Holiday";

        var hasAttendance = statuses.Any();
        if (IsWeekend(date) && !hasAttendance)
            return "Holiday";

        if (statuses.Contains(AttendanceStatus.Present))
            return "Present";

        if (statuses.Contains(AttendanceStatus.Absent))
            return "Absent";

        if (statuses.Contains(AttendanceStatus.Late))
            return "Late";

        if (statuses.Contains(AttendanceStatus.HalfDay))
            return "HalfDay";

        if (statuses.Contains(AttendanceStatus.Leave))
            return "Leave";

        return "Holiday";
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
                a.EmployeeId == employeeId &&
                a.Date.Date == DateTime.Today)
            .OrderByDescending(a => a.Id)
            .FirstOrDefaultAsync();
    }

}
