using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
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

    private static readonly DateTime SystemStartDate = DateTime.UtcNow;

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

    public async Task<ServiceResult<List<AttendanceDto>>> GetEmployeeAttendanceByStatusAsync(AttendanceStatus? status, int? employeeId)
    {
        var data = await _attendanceRepository.GetEmployeeAttendanceByStatus(status, employeeId);

        return ServiceResult<List<AttendanceDto>>.Success(_mapper.Map<List<AttendanceDto>>(data));
    }

    public async Task<ServiceResult<AttendanceDto?>> GetAttendanceByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<AttendanceDto?>.Fail("Invalid Attendance ID.");

        var attendance = await _attendanceRepository.GetAttendanceByIdAsync(id);

        if (attendance == null)
            return ServiceResult<AttendanceDto?>.Fail("Attendance not found.");

        return ServiceResult<AttendanceDto?>.Success(_mapper.Map<AttendanceDto>(attendance));
    }

    public async Task<ServiceResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto attendanceDto)
    {
        if (attendanceDto == null)
            return ServiceResult<AttendanceDto>.Fail("Invalid request.");

        var validation = await _validator.ValidateCreateAsync(attendanceDto);
        if (!validation.IsValid)
            return ServiceResult<AttendanceDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var employeeResult = await _employeeService.GetEmployeeByIdAsync(attendanceDto.EmployeeId);

        if (!employeeResult.IsSuccess || employeeResult.Data == null)
            return ServiceResult<AttendanceDto>.Fail("Employee not found.");

        var entity = _mapper.Map<Attendance>(attendanceDto);

        await CreateAbsentAndHolidayAsync(entity.EmployeeId, entity.Date, employeeResult.Data.HireDate);

        var created = await _attendanceRepository.CreateAttendanceAsync(entity);

        return ServiceResult<AttendanceDto>.Success(_mapper.Map<AttendanceDto>(created), "Attendance created successfully.");
    }

    public async Task<ServiceResult<List<MonthlyAttendanceDto>>> GetAttendanceByMonthAsync(int year, int month, int? employeeId, DateTime? date, AttendanceStatus? status)
    {
        if (year <= 0)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid year.");

        if (month < 1 || month > 12)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid month.");

        if (employeeId.HasValue && employeeId > 0)
        {
            var exist = await _employeeService.GetEmployeeByIdAsync(employeeId.Value);

            if (!exist.IsSuccess || exist.Data == null)
                return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Employee not found.");
        }

        var result = await _attendanceRepository.GetAttendanceByMonthAsync(year, month, employeeId, date, status);

        return result.Any()
            ? ServiceResult<List<MonthlyAttendanceDto>>.Success(result)
            : ServiceResult<List<MonthlyAttendanceDto>>.Fail("Data not found.");
    }

    public async Task<ServiceResult<AttendanceDto?>> GetTodayLastEntryAsync(int employeeId)
    {
        if (employeeId <= 0)
            return ServiceResult<AttendanceDto?>.Fail("Invalid Employee ID.");

        var exist = await _employeeService.GetEmployeeByIdAsync(employeeId);

        if (!exist.IsSuccess || exist.Data == null)
            return ServiceResult<AttendanceDto?>.Fail("Employee not found.");

        var last = await _attendanceRepository.GetLastAttendanceTodayAsync(employeeId);

        if (last == null)
            return ServiceResult<AttendanceDto?>.Fail("Attendance not found.");

        return ServiceResult<AttendanceDto?>.Success(_mapper.Map<AttendanceDto>(last));
    }

    private async Task CreateAbsentAndHolidayAsync(int employeeId, DateTime newAttendanceDate, DateTime hireDate)
    {
        var lastAttendanceDate = await _attendanceRepository.GetLastAttendanceDateAsync(employeeId);

        var effectiveStartDate =
            lastAttendanceDate?.Date
            ?? (hireDate.Date > SystemStartDate.Date
                ? hireDate.Date
                : SystemStartDate.Date);

        if (effectiveStartDate >= newAttendanceDate.Date)
            return;

        var festivalDates = (await _festivalHolidayService
            .GetFestivalHolidaysAsync())
            .Data?
            .Select(f => f.Date.Date)
            .ToHashSet() ?? new HashSet<DateTime>();

        var leaveDates = (await _leaveService
            .GetLeaveRequestsAsync())
            .Data?
            .Where(l => l.EmployeeId == employeeId && l.StatusId == Status.Approved)
            .SelectMany(l => Enumerable.Range(
                0,
                (l.EndDate.Date - l.StartDate.Date).Days + 1)
                .Select(offset => l.StartDate.Date.AddDays(offset)))
            .ToHashSet() ?? new HashSet<DateTime>();

        var missingDates = Enumerable
            .Range(1, (newAttendanceDate.Date - effectiveStartDate).Days - 1)
            .Select(i => effectiveStartDate.AddDays(i));

        foreach (var date in missingDates)
        {
            bool isHoliday = IsWeekend(date) || festivalDates.Contains(date);
            bool isLeave = leaveDates.Contains(date);

            if (!isLeave && !isHoliday)
            {
                await _attendanceRepository.CreateAttendanceAsync(new Attendance
                {
                    EmployeeId = employeeId,
                    Date = date,
                    ClockIn = null,
                    ClockOut = null,
                    TotalHours = TimeSpan.Zero,
                    StatusId = AttendanceStatus.Absent,
                    Notes = "Absent",
                    Latitude = null,
                    Longitude = null,
                    CreatedOn = DateTime.UtcNow
                });
            }
        }
    }

    private static bool IsWeekend(DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}