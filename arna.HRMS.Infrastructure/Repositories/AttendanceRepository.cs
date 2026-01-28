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


    public AttendanceRepository(IBaseRepository<Attendance> baseRepository, FestivalHolidayRepository festivalHolidayRepository)
    {
        _baseRepository = baseRepository;
        _festivalHolidayRepository = festivalHolidayRepository;
    }

    public async Task<List<Attendance>> GetAttendenceAsync()
    {
        return await _baseRepository.Query()
            .Include(x => x.Employee)
            .OrderByDescending(x => x.Id)
            .ToListAsync();
    }

    public async Task<Attendance?> GetAttendanceByIdAsync(int id)
    {
        var attendence = await _baseRepository.GetByIdAsync(id);
        return attendence;
    }

    public Task<Attendance> CreateAttendanceAsync(Attendance attendance)
    {
        return _baseRepository.AddAsync(attendance);
    }

    public async Task<IEnumerable<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int EmpId)
    {
        /*var allAttendance = await _baseRepository.GetAllAsync();
        return allAttendance.Where(a => a.Date.Year == year && a.Date.Month == month && a.EmployeeId == EmpId);*/
        // 1️⃣ Attendance
        var attendances = await _baseRepository.Query()
            .Where(a =>
                a.EmployeeId == EmpId &&
                a.IsActive &&
                !a.IsDeleted &&
                a.Date.Year == year &&
                a.Date.Month == month)
            .ToListAsync();

        // 2️⃣ Festival holidays
        var festivalDates = (await _festivalHolidayRepository.GetByMonthAsync(year, month))
            .Select(fh => fh.Date.Date)
            .ToList();

        // 3️⃣ Weekends
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var weekendDates = Enumerable
            .Range(0, (endDate - startDate).Days + 1)
            .Select(d => startDate.AddDays(d))
            .Where(d => d.DayOfWeek == DayOfWeek.Saturday ||
                        d.DayOfWeek == DayOfWeek.Sunday)
            .Select(d => d.Date)
            .ToList();

        // 4️⃣ All dates (Attendance + Festival + Weekend)
        var allDates = attendances
            .Select(a => a.Date.Date)
            .Union(festivalDates)
            .Union(weekendDates)
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        // 5️⃣ Group attendance by date
        var attendanceByDate = attendances
            .GroupBy(a => a.Date.Date)
            .ToDictionary(g => g.Key, g => g.ToList());

        // 6️⃣ Build result
        var result = new List<MonthlyAttendanceDto>();

        foreach (var date in allDates)
        {
            attendanceByDate.TryGetValue(date, out var dailyRecords);

            var clockIn = dailyRecords?
                .Where(x => x.ClockIn.HasValue)
                .Min(x => x.ClockIn);

            var clockOut = dailyRecords?
                .Where(x => x.ClockOut.HasValue)
                .Max(x => x.ClockOut);

            var workingSeconds = dailyRecords?
                .Where(x => x.TotalHours.HasValue)
                .Sum(x => x.TotalHours.Value.TotalSeconds) ?? 0;

            var totalSeconds =
                (clockIn.HasValue && clockOut.HasValue)
                    ? (clockOut.Value - clockIn.Value).TotalSeconds
                    : 0;

            var breakSeconds = Math.Max(0, totalSeconds - workingSeconds);

            var statuses = dailyRecords?
                .Select(x => x.Status)
                .Distinct()
                .ToList() ?? new List<AttendanceStatus>();

            var status = CalculateStatus(
                statuses,
                festivalDates.Contains(date),
                DateOnly.FromDateTime(date)
            );

            result.Add(new MonthlyAttendanceDto
            {
                EmployeeId = EmpId,
                Date = DateOnly.FromDateTime(date),
                Day = date.DayOfWeek.ToString(),
                ClockIn = clockIn?.TimeOfDay,
                ClockOut = clockOut?.TimeOfDay,
                WorkingHours = TimeSpan.FromSeconds(workingSeconds),
                BreakDuration = TimeSpan.FromSeconds(breakSeconds),
                TotalHours = TimeSpan.FromSeconds(totalSeconds),
                Status = status
            });
        }

        return result;
    }


    private static string CalculateStatus(IEnumerable<AttendanceStatus> statuses, bool isFestivalHoliday, DateOnly date)
    {
        if (isFestivalHoliday)
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

        var hasAttendance = statuses.Any();
        if (IsWeekend(date) && !hasAttendance)
            return "Holiday";

        return "Absent";
    }


    private static bool IsWeekend(DateOnly date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday ||
               date.DayOfWeek == DayOfWeek.Sunday;
    }

    public async Task<DateTime?> GetLastAttendanceDateAsync(int employeeId)
    {
        return await _baseRepository.Query()
            .Where(x => x.EmployeeId == employeeId && x.Status == AttendanceStatus.Present)
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
