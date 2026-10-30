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

    // Bug 2 FIX: Use a fixed business start date instead of DateTime.UtcNow which changes on every restart
    private static readonly DateTime SystemStartDate = new DateTime(2025, 1, 1);

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

    public async Task<ServiceResult<List<AttendanceDto>>> GetAttendanceByStatusAndEmployeeIdAsync(AttendanceStatus? status, int? employeeId)
    {
        var data = await _attendanceRepository.GetAttendanceByStatusAndEmployeeId(status, employeeId);

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
        if (entity.DeviceId == null || entity.DeviceId == 0)
        {
            entity.DeviceId = DeviceType.LaptopDesktop;
        }

        await CreateAbsentAndHolidayAsync(entity.EmployeeId, entity.Date, employeeResult.Data.HireDate);

        // Use Case 4: If employee clocks in on a date covered by an approved leave (pre-applied or auto-marked),
        // automatically revoke the leave for that date, soft-delete the leave attendance entry, and refund leave balance
        if (entity.StatusId == AttendanceStatus.Present || entity.ClockIn != null)
        {
            await _leaveService.RevokeLeaveRequestAsync(entity.EmployeeId, entity.Date, entity.Date);
        }

        var created = await _attendanceRepository.CreateAttendanceAsync(entity);

        return ServiceResult<AttendanceDto>.Success(_mapper.Map<AttendanceDto>(created), "Attendance created successfully.");
    }

    public async Task<ServiceResult<List<MonthlyAttendanceDto>>> GetAttendanceByMonthAsync(int year, int month, int? employeeId, DateTime? date, AttendanceStatus? statusId)
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

        var result = await _attendanceRepository.GetAttendanceByMonthAsync(year, month, employeeId, date, statusId);

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

        // Perf 1 FIX: Filter holidays by the missing date range instead of loading ALL holidays
        var festivalHolidaysResult = await _festivalHolidayService.GetFestivalHolidaysAsync();
        var festivalDates = festivalHolidaysResult.Data?
            .Where(f => f.Date.Date > effectiveStartDate && f.Date.Date < newAttendanceDate.Date)
            .Select(f => f.Date.Date)
            .ToHashSet() ?? new HashSet<DateTime>();

        // Perf 1 FIX: Filter leave requests by employee ID and date range instead of loading ALL
        var leaveRequestsResult = await _leaveService.GetLeaveRequestsByFilterAsync(Status.Approved, employeeId);
        var leaveDates = leaveRequestsResult.Data?
            .Where(l => l.EndDate.Date > effectiveStartDate && l.StartDate.Date < newAttendanceDate.Date)
            .SelectMany(l => Enumerable.Range(
                0,
                (l.EndDate.Date - l.StartDate.Date).Days + 1)
                .Select(offset => l.StartDate.Date.AddDays(offset)))
            .ToHashSet() ?? new HashSet<DateTime>();

        // Pre-fetch leave type BEFORE the loop using await (avoids .Result deadlock)
        var leaveTypesResult = await _leaveService.GetLeaveTypesAsync();
        var defaultLeaveTypeId = leaveTypesResult.Data?.FirstOrDefault()?.Id ?? 0;

        var missingDates = Enumerable
            .Range(1, (newAttendanceDate.Date - effectiveStartDate).Days - 1)
            .Select(i => effectiveStartDate.AddDays(i));

        var datesToMarkAsLeave = new List<DateTime>();

        foreach (var date in missingDates)
        {
            bool isHoliday = IsWeekend(date) || festivalDates.Contains(date);
            bool isLeave = leaveDates.Contains(date);

            // Auto-mark daily Attendance as Leave
            if (!isLeave && !isHoliday)
            {
                await _attendanceRepository.CreateAttendanceAsync(new Attendance
                {
                    EmployeeId = employeeId,
                    Date = date,
                    ClockIn = null,
                    ClockOut = null,
                    TotalHours = TimeSpan.Zero,
                    StatusId = AttendanceStatus.Leave,
                    Notes = "Auto Leave Marked",
                    Latitude = null,
                    Longitude = null,
                    DeviceId = DeviceType.LaptopDesktop
                });

                datesToMarkAsLeave.Add(date.Date);
            }
        }

        // Group contiguous missing dates into single unified LeaveRequest records
        if (datesToMarkAsLeave.Any())
        {
            var ranges = new List<(DateTime Start, DateTime End)>();
            DateTime currentStart = datesToMarkAsLeave[0];
            DateTime currentEnd = datesToMarkAsLeave[0];

            for (int i = 1; i < datesToMarkAsLeave.Count; i++)
            {
                var nextDate = datesToMarkAsLeave[i];
                // Check if the gap between currentEnd and nextDate consists only of weekends/holidays
                bool canBridge = true;
                for (var checkDate = currentEnd.AddDays(1); checkDate < nextDate; checkDate = checkDate.AddDays(1))
                {
                    bool isH = IsWeekend(checkDate) || festivalDates.Contains(checkDate);
                    if (!isH)
                    {
                        canBridge = false;
                        break;
                    }
                }

                if (canBridge)
                {
                    currentEnd = nextDate;
                }
                else
                {
                    ranges.Add((currentStart, currentEnd));
                    currentStart = nextDate;
                    currentEnd = nextDate;
                }
            }
            ranges.Add((currentStart, currentEnd));

            foreach (var range in ranges)
            {
                await _leaveService.CreateLeaveRequestAsync(new LeaveRequestDto
                {
                    EmployeeId = employeeId,
                    LeaveTypeId = defaultLeaveTypeId,
                    StartDate = range.Start,
                    EndDate = range.End,
                    Reason = "Auto Leave Marked",
                    StatusId = Status.Approved,
                    ApprovalNotes = "Auto Approved",
                    ApprovedDate = DateTime.Now.Date
                });
            }
        }
    }

    public async Task<ServiceResult<List<AttendanceDto>>> GetEmployeeTodayFirstClockInAsync(int employeeId)
    {
        if (employeeId <= 0)
            return ServiceResult<List<AttendanceDto>>.Fail("Invalid Employee ID.");

        var exist = await _employeeService.GetEmployeeByIdAsync(employeeId);
        if (!exist.IsSuccess || exist.Data == null)
            return ServiceResult<List<AttendanceDto>>.Fail("Employee not found.");

        var firstClockIn = await _attendanceRepository.GetTodayFirstClockInAsync(employeeId);

        if (firstClockIn == null)
            return ServiceResult<List<AttendanceDto>>.Fail("Attendance not found.");

        return ServiceResult<List<AttendanceDto>>.Success(_mapper.Map<List<AttendanceDto>>(firstClockIn));

    }

    private static bool IsWeekend(DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}