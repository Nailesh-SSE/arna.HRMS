using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AttendanceRepository _attendanceRepository;
    private readonly IMapper _mapper;
    private readonly IEmployeeService _employeeService;
    private readonly IFestivalHolidayService _festivalHolidayService;

    private static readonly DateTime SystemStartDate = new(2026, 1, 12);

    public AttendanceService(
        AttendanceRepository attendanceRepository,
        IMapper mapper,
        IEmployeeService employeeService,
        IFestivalHolidayService festivalHolidayService)
    {
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
        _employeeService = employeeService;
        _festivalHolidayService = festivalHolidayService;
    }

    #region Basic APIs

    public async Task<List<AttendanceDto>> GetAttendanceAsync()
    {
        var data = await _attendanceRepository.GetAttendenceAsync();
        return _mapper.Map<List<AttendanceDto>>(data);
    }

    public async Task<AttendanceDto?> GetAttendenceByIdAsync(int id)
    {
        var attendance = await _attendanceRepository.GetAttendanceByIdAsync(id);
        return attendance == null ? null : _mapper.Map<AttendanceDto>(attendance);
    }

    public async Task<AttendanceDto> CreateAttendanceAsync(AttendanceDto attendanceDto)
    {
        var entity = _mapper.Map<Attendance>(attendanceDto);

        var employee =
            await _employeeService.GetEmployeeByIdAsync(entity.EmployeeId);

        await CreateAbsentAndHolidayAsync(
            entity.EmployeeId,
            entity.Date,
            employee.HireDate);

        var created =
            await _attendanceRepository.CreateAttendanceAsync(entity);

        return _mapper.Map<AttendanceDto>(created);
    }

    #endregion

    #region Monthly Attendance

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(
        int year,
        int month,
        int empId)
    {
        var attendances =
            await _attendanceRepository.GetAttendanceByMonthAsync(year, month, empId);

        var festivals =
            await _festivalHolidayService.GetFestivalHolidayByMonthAsync(year,month);

        var monthFestivalDates = festivals
            .Select(f => f.Date.Date)
            .ToHashSet();

        // ✅ FORCE nullable DateTime on BOTH sides
        var combined = attendances
            .Select(a => new
            {
                a.EmployeeId,
                Date = a.Date.Date,
                ClockIn = (DateTime?)a.ClockIn,
                ClockOut = (DateTime?)a.ClockOut
            })
            .Concat(
                monthFestivalDates.Select(d => new
                {
                    EmployeeId = empId,
                    Date = d,
                    ClockIn = (DateTime?)null,
                    ClockOut = (DateTime?)null
                })
            );

        var result = combined
            .GroupBy(x => new { x.EmployeeId, x.Date })
            .Select(g =>
            {
                var date = DateOnly.FromDateTime(g.Key.Date);

                var clockInTimes = g
                    .Where(x => x.ClockIn.HasValue)
                    .Select(x => x.ClockIn!.Value.TimeOfDay);

                var clockOutTimes = g
                    .Where(x => x.ClockOut.HasValue)
                    .Select(x => x.ClockOut!.Value.TimeOfDay);

                TimeSpan? minClockIn =
                    clockInTimes.Any() ? clockInTimes.Min() : null;

                TimeSpan? maxClockOut =
                    clockOutTimes.Any() ? clockOutTimes.Max() : null;

                var totalHours =
                    minClockIn.HasValue && maxClockOut.HasValue
                        ? maxClockOut.Value - minClockIn.Value
                        : TimeSpan.Zero;

                bool isFestivalHoliday =
                    monthFestivalDates.Contains(g.Key.Date);

                return new MonthlyAttendanceDto
                {
                    EmployeeId = g.Key.EmployeeId,
                    Date = date,
                    Day = g.Key.Date.DayOfWeek.ToString(),
                    ClockIn = minClockIn,
                    ClockOut = maxClockOut,
                    TotalHours = totalHours,
                    Status = CalculateStatus(
                        minClockIn,
                        date,
                        isFestivalHoliday)
                };
            })
            .OrderBy(x => x.Date)
            .ToList();

        return result;
    }

    #endregion

    #region Status Logic

    private static string CalculateStatus(
        TimeSpan? clockIn,
        DateOnly date,
        bool isFestivalHoliday)
    {
        if (isFestivalHoliday || IsWeekend(date))
            return "Holiday";

        if (clockIn.HasValue && clockIn.Value != TimeSpan.Zero)
            return "Present";

        return "Absent";
    }
    /*
    private static AttendanceStatus CalculateStatus(TimeSpan? clockIn, double totalHours, DateOnly date,
        bool isFestivalHoliday)
    {
        var officeStart = new TimeSpan(12, 0, 0);   // 12:00 PM
        var officeEnd = new TimeSpan(19, 30, 0);  // 7:30 PM
        var fullDayHours = (officeEnd - officeStart).TotalHours; // 7.5
        if (isFestivalHoliday || IsWeekend(date))
            return AttendanceStatus.Holiday;
        if (!clockIn.HasValue)
            return AttendanceStatus.Absent;

        if (totalHours < 3)
            return AttendanceStatus.Absent;

        if (totalHours >= 4 && totalHours < fullDayHours)
            return AttendanceStatus.HalfDay;

        var lateThreshold = officeStart.Add(TimeSpan.FromMinutes(15));
        if (clockIn.Value > lateThreshold)
            return AttendanceStatus.Late;

        return AttendanceStatus.Present;
    }*/

    #endregion

    #region Auto Absent & Holiday Creation

    private async Task CreateAbsentAndHolidayAsync(
        int employeeId,
        DateTime newAttendanceDate,
        DateTime hireDate)
    {
        var lastAttendanceDate =
            await _attendanceRepository.GetLastAttendanceDateAsync(employeeId);

        var effectiveStartDate =
            lastAttendanceDate?.Date
            ?? MaxDate(hireDate.Date, SystemStartDate);

        if (effectiveStartDate >= newAttendanceDate.Date)
            return;

        var festivalDates =
            (await _festivalHolidayService.GetFestivalHolidayAsync())
            .Select(f => f.Date.Date)
            .ToHashSet();

        var missingDates = Enumerable
            .Range(1, (newAttendanceDate.Date - effectiveStartDate).Days - 1)
            .Select(i => effectiveStartDate.AddDays(i));

        foreach (var date in missingDates)
        {
            bool isHoliday =
                IsWeekend(date) || festivalDates.Contains(date.Date);

            await _attendanceRepository.CreateAttendanceAsync(new Attendance
            {
                EmployeeId = employeeId,
                Date = date,
                ClockIn = null,
                ClockOut = null,
                TotalHours = TimeSpan.Zero,
                Status = isHoliday
                    ? AttendanceStatus.Holiday
                    : AttendanceStatus.Absent,
                Notes = isHoliday
                    ? "Holiday"
                    : "Absent"
            });
        }
    }

    #endregion

    #region Today

    public async Task<AttendanceDto?> GetTodayLastEntryAsync(int employeeId)
    {
        var last =
            await _attendanceRepository.GetLastAttendanceTodayAsync(employeeId);

        return last == null ? null : _mapper.Map<AttendanceDto>(last);
    }

    #endregion

    #region Helpers

    private static bool IsWeekend(DateOnly date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static bool IsWeekend(DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static DateTime MaxDate(DateTime a, DateTime b) =>
        a > b ? a : b;

    #endregion
}
