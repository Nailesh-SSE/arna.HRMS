using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace arna.HRMS.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AttendanceRepository _attendanceRepository;
    private readonly IMapper _mapper;
    private readonly IEmployeeService _employeeService;
    private readonly IFestivalHolidayService _festivalHolidayService;
    private readonly ILeaveService _leaveService;


    private static readonly DateTime SystemStartDate = new(2026, 2, 1);

    public AttendanceService(
        AttendanceRepository attendanceRepository,
        IMapper mapper,
        IEmployeeService employeeService,
        IFestivalHolidayService festivalHolidayService,
        ILeaveService leaveService)
    {
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
        _employeeService = employeeService;
        _festivalHolidayService = festivalHolidayService;
        _leaveService = leaveService;
    }

    #region Basic APIs

    public async Task<ServiceResult<List<AttendanceDto>>> GetAttendanceAsync()
    {
        var data = await _attendanceRepository.GetAttendenceAsync();
        var list = _mapper.Map<List<AttendanceDto>>(data);

        return ServiceResult<List<AttendanceDto>>.Success(list);
    }

    public async Task<ServiceResult<AttendanceDto?>> GetAttendenceByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<AttendanceDto?>.Fail("Invalid Attendance ID");

        var attendance = await _attendanceRepository.GetAttendanceByIdAsync(id);

        if (attendance == null)
            return ServiceResult<AttendanceDto?>.Fail("Attendance not found");

        var dto = _mapper.Map<AttendanceDto>(attendance);
        return dto!=null
            ? ServiceResult<AttendanceDto?>.Success(dto)
            : ServiceResult<AttendanceDto?>.Fail("Fail to find Attendance");
    }

    public async Task<ServiceResult<AttendanceDto>> CreateAttendanceAsync([FromBody] AttendanceDto attendanceDto)
    {
        if (attendanceDto == null)
            return ServiceResult<AttendanceDto>.Fail("Invalid request");

        if (attendanceDto.EmployeeId <= 0)
            return ServiceResult<AttendanceDto>.Fail("EmployeeId is required");
        if (attendanceDto.Date == default)
            return ServiceResult<AttendanceDto>.Fail("Date is Required.");
        if (attendanceDto.Notes == default)
            return ServiceResult<AttendanceDto>.Fail("Note is Required.");

        var entity = _mapper.Map<Attendance>(attendanceDto);

        // employeeService in your project returns ServiceResult<EmployeeDto?>
        var employeeResult = await _employeeService.GetEmployeeByIdAsync(entity.EmployeeId);

        if (!employeeResult.IsSuccess || employeeResult.Data == null)
            return ServiceResult<AttendanceDto>.Fail("Employee not found");

        await CreateAbsentAndHolidayAsync(
            entity.EmployeeId,
            entity.Date,
            employeeResult.Data.HireDate);

        var created = await _attendanceRepository.CreateAttendanceAsync(entity);

        var resultDto = _mapper.Map<AttendanceDto>(created);
        return resultDto!=null
            ? ServiceResult<AttendanceDto>.Success(resultDto, "Attendance created successfully")
            : ServiceResult<AttendanceDto>.Fail("Fail to Create Attendance");
    }

    #endregion

    #region Monthly Attendance Api
    public async Task<ServiceResult<List<MonthlyAttendanceDto>>> GetAttendanceByMonthAsync(int year, int month, int? empId, DateTime? date)
    {
        if (year <= 0)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid year");

        if (month < 1 || month > 12)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid month");

        var attendances = await _attendanceRepository.GetAttendanceByMonthAsync(year, month, empId, date);

        return attendances.Any()
            ? ServiceResult<List<MonthlyAttendanceDto>>.Success((List<MonthlyAttendanceDto>)attendances)
            : ServiceResult<List<MonthlyAttendanceDto>>.Fail("Data Not Found");

    }
    #endregion

    #region Monthly Attendance

    /*public async Task<ServiceResult<List<MonthlyAttendanceDto>>> GetAttendanceByMonthAsync(
        int year,
        int month,
        int empId)
    {
        if (year <= 0)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid year");

        if (month < 1 || month > 12)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid month");

        if (empId <= 0)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid Employee ID");

        var attendances =
            await _attendanceRepository.GetAttendanceByMonthAsync(year, month, empId);

        var festivalsResult =
            await _festivalHolidayService.GetFestivalHolidayByMonthAsync(year, month);

        if (!festivalsResult.IsSuccess || festivalsResult.Data == null)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail(festivalsResult.Message);

        var monthFestivalDates = festivalsResult.Data
            .Select(f => f.Date.Date)
            .ToHashSet();

        var combined = attendances
            .Select(a => new
            {
                a.EmployeeId,
                Date = a.Date.Date,
                ClockIn = (DateTime?)a.ClockIn,
                ClockOut = (DateTime?)a.ClockOut,

            })
            .Concat(
                monthFestivalDates.Select(d => new
                {
                    EmployeeId = empId,
                    Date = d,
                    ClockIn = (DateTime?)null,
                    ClockOut = (DateTime?)null,
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

                bool isFestivalHoliday = monthFestivalDates.Contains(g.Key.Date);

                var dailyStatuses = attendances
                .Where(a =>
                    a.EmployeeId == g.Key.EmployeeId &&
                    a.Date.Date == g.Key.Date
                )
                .Select(a => a.Status)
                .ToList();

                return new MonthlyAttendanceDto
                {
                    EmployeeId = g.Key.EmployeeId,
                    Date = date,
                    Day = g.Key.Date.DayOfWeek.ToString(),
                    ClockIn = minClockIn,
                    ClockOut = maxClockOut,
                    TotalHours = totalHours,
                    Status = CalculateStatus(dailyStatuses, isFestivalHoliday, date)
                };

            })
            .OrderBy(x => x.Date)
            .ToList();

        return ServiceResult<List<MonthlyAttendanceDto>>.Success(result);
    }*/

    #endregion

    #region Status Logic

    /*private static String CalculateStatus(
    IEnumerable<AttendanceStatus> statuses,
    bool isFestivalHoliday,
    DateOnly date)
    {
        if (isFestivalHoliday || IsWeekend(date))
            return "Holiday";

        if (statuses.Contains(AttendanceStatus.Leave))
            return "Leave";

        if (statuses.Contains(AttendanceStatus.Present))
            return "Present";

        return "Absent";
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

        var festivalResult = await _festivalHolidayService.GetFestivalHolidayAsync();
        var approvedLeaves = (await _leaveService.GetLeaveRequestAsync()).Data?
                .Where(l =>
                    l.EmployeeId == employeeId &&
                    l.StatusId == Status.Approved)
                .SelectMany(l =>
                    Enumerable.Range(
                        0,
                        (l.EndDate.Date - l.StartDate.Date).Days + 1
                    )
                    .Select(offset => l.StartDate.Date.AddDays(offset))
                )
                .ToHashSet() ?? new HashSet<DateTime>();

        var festivalDates = (festivalResult.Data ?? new List<FestivalHolidayDto>())
            .Select(f => f.Date.Date)
            .ToHashSet();

        var missingDates = Enumerable
            .Range(1, (newAttendanceDate.Date - effectiveStartDate).Days - 1)
            .Select(i => effectiveStartDate.AddDays(i));

        foreach (var date in missingDates)
        {
            bool isHoliday = IsWeekend(date) || festivalDates.Contains(date.Date);
            bool isLeave = approvedLeaves.Contains(date.Date);

            if (!isLeave && !isHoliday)
            {
                await _attendanceRepository.CreateAttendanceAsync(new Attendance
                {
                    EmployeeId = employeeId,
                    Date = date,
                    ClockIn = null,
                    ClockOut = null,
                    TotalHours = TimeSpan.Zero,
                    StatusId = isHoliday
                    ? AttendanceStatus.Holiday
                    : AttendanceStatus.Absent,
                    Notes = isHoliday
                    ? "Holiday"
                    : "Absent",
                    Latitude = null,
                    Longitude = null

                });
            }
        }
    }

    #endregion

    #region Today

    public async Task<ServiceResult<AttendanceDto?>> GetTodayLastEntryAsync(int employeeId)
    {
        if (employeeId <= 0)
            return ServiceResult<AttendanceDto?>.Fail("Invalid Employee ID");

        var last =
            await _attendanceRepository.GetLastAttendanceTodayAsync(employeeId);

        if (last == null)
            return ServiceResult<AttendanceDto?>.Fail("Attendance not found");

        var dto = _mapper.Map<AttendanceDto>(last);
        return ServiceResult<AttendanceDto?>.Success(dto);
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
