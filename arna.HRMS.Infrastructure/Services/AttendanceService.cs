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

    public AttendanceService(AttendanceRepository attendanceRepository, IMapper mapper)
    {
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
    }

    public async Task<List<AttendanceDto>> GetAttendanceAsync()
    {
        var Attendance = await _attendanceRepository.GetAttendenceAsync();
        return Attendance.Select(e => _mapper.Map<AttendanceDto>(e)).ToList();
    }

    public async Task<AttendanceDto?> GetAttendenceByIdAsync(int id)
    {
        var attendance = await _attendanceRepository.GetAttendanceByIdAsync(id);
        if (attendance == null) return null;
        var attendancedto = _mapper.Map<AttendanceDto>(attendance);

        return attendancedto;
    }

    public async Task<AttendanceDto> CreateAttendanceAsync(Attendance attendance)
    {
        var createdAttendance = await _attendanceRepository.CreateAttendanceDtoAsync(attendance);
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
                var totalHours = clockInTimes.Any() && clockOutTimes.Any()? (clockOutTimes.Max() - clockInTimes.Min()).TotalHours:0;

                TimeSpan? minClockIn = clockInTimes.Any() ? clockInTimes.Min() : null;
                TimeSpan? maxClockOut = clockOutTimes.Any() ? clockOutTimes.Max() : null;

                return new MonthlyAttendanceDto
                {
                    EmployeeId = g.Key.EmployeeId,
                    Date = date,
                    Day = g.Key.Date.DayOfWeek.ToString(),

                    ClockIn = minClockIn,
                    ClockOut = maxClockOut,

                    TotalHours = totalHours,
                    Status = CalculateStatus(minClockIn)
                };
            })
            .OrderBy(x => x.Date)
            .ToList();

        return result;
    }

    private static string CalculateStatus(TimeSpan? clockIn)
    {
        if (clockIn.HasValue && clockIn.Value != TimeSpan.Zero)
            return "Present";

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


}
