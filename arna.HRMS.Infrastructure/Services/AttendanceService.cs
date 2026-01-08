using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Interfaces;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Models.DTOs;
using AutoMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace arna.HRMS.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AttendanceRepository _attendanceRepository;
    private readonly IMapper _mapper;
    private readonly IEmployeeService _employeeService;

    public AttendanceService(AttendanceRepository attendanceRepository, IMapper mapper, IEmployeeService Emp)
    {
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
        _employeeService = Emp;
    }

    public async Task<List<AttendanceDto>> GetAttendanceAsync()
    {
        var Attendance = await _attendanceRepository.GetAttendenceAsync();
        return _mapper.Map<List<AttendanceDto>>(Attendance);
    }

    public async Task<AttendanceDto?> GetAttendenceByIdAsync(int id)
    {
        var attendance = await _attendanceRepository.GetAttendanceByIdAsync(id);
        return attendance == null ? null : _mapper.Map<AttendanceDto>(attendance);
    }
    public async Task<AttendanceDto> CreateAttendanceAsync(AttendanceDto attendanceDto)
    {
        var attendanceEntity = _mapper.Map<Attendance>(attendanceDto);
        var employee = await _employeeService.GetEmployeeByIdAsync(attendanceEntity.EmployeeId);

        await CreateAbsentAndHolidayAsync(attendanceEntity.EmployeeId, attendanceEntity.Date, employee.HireDate);

        var createdAttendance =
            await _attendanceRepository.CreateAttendanceAsync(attendanceEntity);

        return _mapper.Map<AttendanceDto>(createdAttendance);
    }

    public async Task<List<MonthlyAttendanceDto>> GetAttendanceByMonthAsync(int year, int month, int empId)
    {
        var attendances = await _attendanceRepository
            .GetAttendanceByMonthAsync(year, month, empId);

        var result = attendances
            .GroupBy(a => new { a.EmployeeId, Date = a.Date.Date })
            .Select(g =>
            {
                var date = DateOnly.FromDateTime(g.Key.Date);

                var clockInTimes = g
                    .Where(x => x.ClockIn.HasValue)
                    .Select(x => x.ClockIn!.Value.TimeOfDay);

                var clockOutTimes = g
                    .Where(x => x.ClockOut.HasValue)
                    .Select(x => x.ClockOut!.Value.TimeOfDay);

                //var totalHours = g.Sum(x => x.TotalHours?.TotalHours ?? 0);
                var totalHours = clockInTimes.Any() && clockOutTimes.Any() ? (clockOutTimes.Max() - clockInTimes.Min()) : TimeSpan.FromHours(0);

                TimeSpan? minClockIn = clockInTimes.Any() ? clockInTimes.Min() : null;
                TimeSpan? maxClockOut = clockOutTimes.Any() ? clockOutTimes.Max() : null;

                return new MonthlyAttendanceDto
                {
                    EmployeeId = g.Key.EmployeeId,
                    Date = date,
                    Day = g.Key.Date.DayOfWeek.ToString(),

                    ClockIn =minClockIn,
                    ClockOut = maxClockOut,

                    TotalHours = totalHours,
                    Status = CalculateStatus(minClockIn, date)
                };
            })
            .OrderBy(x => x.Date)
            .ToList();

        return result;
    }

    private static string CalculateStatus(TimeSpan? clockIn,DateOnly date)
    {
        bool isWeekend =
                date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday;
        if (clockIn.HasValue && clockIn.Value != TimeSpan.Zero)
            return "Present";
        else if (isWeekend)
            return "Holiday";
        return "Absent";
    }
    /*
    private static AttendanceStatus CalculateStatus(TimeSpan? clockIn, double totalHours)
    {
        var officeStart = new TimeSpan(12, 0, 0);   // 12:00 PM
        var officeEnd = new TimeSpan(19, 30, 0);  // 7:30 PM
        var fullDayHours = (officeEnd - officeStart).TotalHours; // 7.5

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

    private async Task CreateAbsentAndHolidayAsync(int employeeId, DateTime newAttendanceDate, DateTime EmpHire)
    {
        var lastDate = await _attendanceRepository
            .GetLastAttendanceDateAsync(employeeId);
        var systemStartDate = new DateTime(2026, 1, 5);
        DateTime effectiveStartDate;

        if (!lastDate.HasValue)
        {
            effectiveStartDate =
                EmpHire.Date > systemStartDate
                    ? EmpHire.Date
                    : systemStartDate;
        }
        else
        {
            effectiveStartDate = lastDate.Value.Date;
        }

        if (effectiveStartDate >= newAttendanceDate.Date)
        {
            return;
        }

        var missingDates = Enumerable
            .Range(1, (newAttendanceDate.Date - effectiveStartDate).Days - 1)
            .Select(i => effectiveStartDate.AddDays(i));

        foreach (var date in missingDates)
        {
            bool isWeekend =
                date.DayOfWeek == DayOfWeek.Saturday ||
                date.DayOfWeek == DayOfWeek.Sunday;

            var attendance = new Attendance
            {
                EmployeeId = employeeId,
                Date = date,
                ClockIn = null,
                ClockOut = null,
                TotalHours = TimeSpan.Zero,
                Status = isWeekend
                    ? AttendanceStatus.Holiday
                    : AttendanceStatus.Absent,
                Notes = isWeekend
                    ? "Holiday"
                    : "Absent"
            };

            await _attendanceRepository.CreateAttendanceAsync(attendance);
        }
    }

    public async Task<AttendanceDto?> GetTodayLastEntryAsync(int employeeId)
    {
        var last = await _attendanceRepository
            .GetLastAttendanceTodayAsync(employeeId);

        return last == null ? null : _mapper.Map<AttendanceDto>(last);
    }



}
