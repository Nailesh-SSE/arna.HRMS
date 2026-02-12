using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class AttendanceService : IAttendanceService
{
    private readonly AttendanceRepository _attendanceRepository;
    private readonly IMapper _mapper;
    private readonly IEmployeeService _employeeService;
    private readonly IFestivalHolidayService _festivalHolidayService;
    private readonly ILeaveService _leaveService;
    private readonly AttendanceValidator _validator;


    private static readonly DateTime SystemStartDate = new(2026, 2, 1);

    public AttendanceService(
        AttendanceRepository attendanceRepository,
        IMapper mapper,
        IEmployeeService employeeService,
        IFestivalHolidayService festivalHolidayService,
        ILeaveService leaveService,
        AttendanceValidator validator)
    {
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
        _employeeService = employeeService;
        _festivalHolidayService = festivalHolidayService;
        _leaveService = leaveService;
        _validator = validator;
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

    public async Task<ServiceResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto attendanceDto)
    {
        if (attendanceDto == null)
            return ServiceResult<AttendanceDto>.Fail("Invalid request");

        var validation = await _validator.ValidateCreateAsync(attendanceDto);
        if (!validation.IsValid)
            return ServiceResult<AttendanceDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<Attendance>(attendanceDto);

        var employeeResult = await _employeeService.GetEmployeeByIdAsync(entity.EmployeeId);

        if (!employeeResult.IsSuccess || employeeResult.Data == null)
            return ServiceResult<AttendanceDto>.Fail("Employee not found");

        await CreateAbsentAndHolidayAsync(entity.EmployeeId, entity.Date, employeeResult.Data.HireDate);

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

        var festivalDates = new HashSet<DateTime>();
        var leaveDates = new HashSet<DateTime>();

        var festivalResult = await _festivalHolidayService.GetFestivalHolidayAsync();
        if (festivalResult.IsSuccess && festivalResult.Data != null)
        {
            festivalDates = festivalResult.Data.Select(f => f.Date.Date).ToHashSet();
        }

        var leaveResult = await _leaveService.GetLeaveRequestAsync();
        if (leaveResult.IsSuccess && leaveResult.Data != null)
        {
            leaveDates = leaveResult.Data
                .Where(l => l.EmployeeId == employeeId && l.StatusId == Status.Approved)
                .SelectMany(l => Enumerable.Range(0, (l.EndDate.Date - l.StartDate.Date).Days + 1)
                    .Select(offset => l.StartDate.Date.AddDays(offset)))
                .ToHashSet();
        }

        var missingDates = Enumerable
            .Range(1, (newAttendanceDate.Date - effectiveStartDate).Days - 1)
            .Select(i => effectiveStartDate.AddDays(i));

        foreach (var date in missingDates)
        {
            bool isHoliday = IsWeekend(date) || festivalDates.Contains(date.Date);
            bool isLeave = leaveDates.Contains(date.Date);

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
