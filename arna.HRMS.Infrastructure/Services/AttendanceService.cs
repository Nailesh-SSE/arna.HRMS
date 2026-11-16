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
            return ServiceResult<AttendanceDto?>.Fail("Invalid Attendance ID");

        var attendance = await _attendanceRepository.GetAttendanceByIdAsync(id);

        if (attendance == null)
            return ServiceResult<AttendanceDto?>.Fail("Attendance not found");

        var mapped = _mapper.Map<AttendanceDto>(attendance);
        if (mapped == null)
            return ServiceResult<AttendanceDto?>.Fail("Fail to find Attendance");

        return ServiceResult<AttendanceDto?>.Success(mapped);
    }

    public async Task<ServiceResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto attendanceDto)
    {
        if (attendanceDto == null)
            return ServiceResult<AttendanceDto>.Fail("Invalid request");

        var validation = await _validator.ValidateCreateAsync(attendanceDto);
        if (!validation.IsValid)
            return ServiceResult<AttendanceDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var employeeResult = await _employeeService.GetEmployeeByIdAsync(attendanceDto.EmployeeId);

        if (employeeResult == null || !employeeResult.IsSuccess || employeeResult.Data == null)
            return ServiceResult<AttendanceDto>.Fail("Employee not found");

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

        var mapped = _mapper.Map<AttendanceDto>(created);
        if (mapped == null)
            return ServiceResult<AttendanceDto>.Fail("Fail to Create Attendance");

        return ServiceResult<AttendanceDto>.Success(mapped, "Attendance created successfully.");
    }

    public async Task<ServiceResult<List<MonthlyAttendanceDto>>> GetAttendanceByMonthAsync(int year, int month, int? employeeId, DateTime? date, AttendanceStatus? statusId)
    {
        if (year <= 0)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid year");

        if (month < 1 || month > 12)
            return ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid month");

        if (employeeId.HasValue && employeeId > 0)
        {
            var exist = await _employeeService.GetEmployeeByIdAsync(employeeId.Value);

            if (exist == null || !exist.IsSuccess || exist.Data == null)
                return ServiceResult<List<MonthlyAttendanceDto>>.Fail("No such Employee Found");
        }

        var result = await _attendanceRepository.GetAttendanceByMonthAsync(year, month, employeeId, date, statusId);

        return result.Any()
            ? ServiceResult<List<MonthlyAttendanceDto>>.Success(result)
            : ServiceResult<List<MonthlyAttendanceDto>>.Fail("Data not found");
    }

    public async Task<ServiceResult<AttendanceDto?>> GetTodayLastEntryAsync(int employeeId)
    {
        if (employeeId <= 0)
            return ServiceResult<AttendanceDto?>.Fail("Invalid Employee ID");

        var exist = await _employeeService.GetEmployeeByIdAsync(employeeId);

        if (exist == null || !exist.IsSuccess || exist.Data == null)
            return ServiceResult<AttendanceDto?>.Fail("No such Employee Found");

        var last = await _attendanceRepository.GetLastAttendanceTodayAsync(employeeId);

        if (last == null)
            return ServiceResult<AttendanceDto?>.Fail("Attendance not found");

        var mapped = _mapper.Map<AttendanceDto>(last);
        if (mapped == null)
            return ServiceResult<AttendanceDto?>.Fail("No data found");

        return ServiceResult<AttendanceDto?>.Success(mapped);
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

        // Filter festival holidays in the missing date range
        var festivalHolidaysResult = await _festivalHolidayService.GetFestivalHolidaysAsync();
        var festivalDates = festivalHolidaysResult?.Data?
            .Where(f => f.Date.Date > effectiveStartDate && f.Date.Date < newAttendanceDate.Date)
            .Select(f => f.Date.Date)
            .ToHashSet() ?? new HashSet<DateTime>();

        // Query both Approved AND Pending leave requests for this employee so applied leaves are not duplicated
        var leaveRequestsResult = await _leaveService.GetLeaveRequestsByFilterAsync(null, employeeId);
        var leaveDates = leaveRequestsResult?.Data?
            .Where(l => (l.StatusId == Status.Approved || l.StatusId == Status.Pending) &&
                        l.EndDate.Date > effectiveStartDate &&
                        l.StartDate.Date < newAttendanceDate.Date)
            .SelectMany(l => Enumerable.Range(
                0,
                (l.EndDate.Date - l.StartDate.Date).Days + 1)
                .Select(offset => l.StartDate.Date.AddDays(offset)))
            .ToHashSet() ?? new HashSet<DateTime>();

        // Retrieve leave types to determine admin-allowed Paid and Unpaid leave types
        var leaveTypesResult = await _leaveService.GetLeaveTypesAsync();
        var allLeaveTypes = leaveTypesResult?.Data ?? new List<LeaveTypeDto>();
        var activeLeaveTypes = allLeaveTypes.Where(x => x.IsActive).ToList();

        // 1. Paid Leave: configured as LeaveName.PaidLeave, or active leave type with IsPaid == true
        var paidLeaveType = activeLeaveTypes.FirstOrDefault(x => x.LeaveNameId == LeaveName.PaidLeave)
            ?? activeLeaveTypes.FirstOrDefault(x => x.IsPaid);

        // 2. Unpaid Leave: configured as LeaveName.UnpaidLeave, or active leave type with IsPaid == false
        var unpaidLeaveType = activeLeaveTypes.FirstOrDefault(x => x.LeaveNameId == LeaveName.UnpaidLeave)
            ?? activeLeaveTypes.FirstOrDefault(x => !x.IsPaid);

        if (paidLeaveType != null && unpaidLeaveType != null && paidLeaveType.Id == unpaidLeaveType.Id)
        {
            unpaidLeaveType = null;
        }

        // Fallback if neither specific Paid nor Unpaid leave type is active
        var fallbackLeaveType = activeLeaveTypes.FirstOrDefault(x => x.LeaveNameId == LeaveName.CasualLeave)
            ?? activeLeaveTypes.FirstOrDefault()
            ?? allLeaveTypes.FirstOrDefault();

        var missingDates = Enumerable
            .Range(1, (newAttendanceDate.Date - effectiveStartDate).Days - 1)
            .Select(i => effectiveStartDate.AddDays(i))
            .Where(date => !IsWeekend(date) && !festivalDates.Contains(date) && !leaveDates.Contains(date))
            .ToList();

        var datesToMarkAsLeave = new List<DateTime>();

        foreach (var date in missingDates)
        {
            if (!await _attendanceRepository.HasAttendanceOnDateAsync(employeeId, date))
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

        // Group and create unified LeaveRequest records under Paid Leave and/or Unpaid Leave
        if (datesToMarkAsLeave.Any())
        {
            var paidDates = new List<DateTime>();
            var unpaidDates = new List<DateTime>();
            var fallbackDates = new List<DateTime>();

            if (paidLeaveType != null && unpaidLeaveType != null)
            {
                // Case 1: Admin allows BOTH Paid Leave and Unpaid Leave
                // First deduct from Paid Leave until paid quota (MaxPerYear) is completed, then deduct from Unpaid Leave.
                var today = DateTime.Today;
                int fyYear = today.Month >= 4 ? today.Year : today.Year - 1;
                var fyStart = new DateTime(fyYear, 4, 1);
                var fyEnd = new DateTime(fyYear + 1, 3, 31);

                int usedPaidDays = leaveRequestsResult?.Data?
                    .Where(l => l.StatusId == Status.Approved &&
                                l.LeaveTypeId == paidLeaveType.Id &&
                                l.StartDate >= fyStart &&
                                l.StartDate <= fyEnd)
                    .Sum(l => l.LeaveDays) ?? 0;

                int paidQuota = paidLeaveType.MaxPerYear;
                int remainingPaidDays = Math.Max(0, paidQuota - usedPaidDays);

                paidDates = datesToMarkAsLeave.Take(remainingPaidDays).ToList();
                unpaidDates = datesToMarkAsLeave.Skip(remainingPaidDays).ToList();
            }
            else if (paidLeaveType != null)
            {
                // Case 2: Admin ONLY allows Paid Leave
                // All leave is deducted from Paid Leave (not mandatory to check if completed; balance can go negative).
                paidDates.AddRange(datesToMarkAsLeave);
            }
            else if (unpaidLeaveType != null)
            {
                // Case 3: Admin ONLY allows Unpaid Leave
                // All leave is deducted from Unpaid Leave.
                unpaidDates.AddRange(datesToMarkAsLeave);
            }
            else if (fallbackLeaveType != null)
            {
                fallbackDates.AddRange(datesToMarkAsLeave);
            }

            if (paidDates.Any() && paidLeaveType != null)
            {
                var paidRanges = GroupDatesIntoRanges(paidDates, festivalDates);
                foreach (var range in paidRanges)
                {
                    await _leaveService.CreateLeaveRequestAsync(new LeaveRequestDto
                    {
                        EmployeeId = employeeId,
                        LeaveTypeId = paidLeaveType.Id,
                        StartDate = range.Start,
                        EndDate = range.End,
                        Reason = "Auto Leave Marked",
                        StatusId = Status.Approved,
                        ApprovalNotes = "Auto Approved",
                        ApprovedDate = DateTime.Now.Date
                    });
                }
            }

            if (unpaidDates.Any() && unpaidLeaveType != null)
            {
                var unpaidRanges = GroupDatesIntoRanges(unpaidDates, festivalDates);
                foreach (var range in unpaidRanges)
                {
                    await _leaveService.CreateLeaveRequestAsync(new LeaveRequestDto
                    {
                        EmployeeId = employeeId,
                        LeaveTypeId = unpaidLeaveType.Id,
                        StartDate = range.Start,
                        EndDate = range.End,
                        Reason = "Auto Leave Marked",
                        StatusId = Status.Approved,
                        ApprovalNotes = "Auto Approved",
                        ApprovedDate = DateTime.Now.Date
                    });
                }
            }

            if (fallbackDates.Any() && fallbackLeaveType != null)
            {
                var fallbackRanges = GroupDatesIntoRanges(fallbackDates, festivalDates);
                foreach (var range in fallbackRanges)
                {
                    await _leaveService.CreateLeaveRequestAsync(new LeaveRequestDto
                    {
                        EmployeeId = employeeId,
                        LeaveTypeId = fallbackLeaveType.Id,
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
    }

    private static List<(DateTime Start, DateTime End)> GroupDatesIntoRanges(List<DateTime> dates, HashSet<DateTime> festivalDates)
    {
        var ranges = new List<(DateTime Start, DateTime End)>();
        if (!dates.Any()) return ranges;

        DateTime currentStart = dates[0];
        DateTime currentEnd = dates[0];

        for (int i = 1; i < dates.Count; i++)
        {
            var nextDate = dates[i];
            bool canBridge = Enumerable
                .Range(1, (nextDate.Date - currentEnd.Date).Days - 1)
                .Select(offset => currentEnd.Date.AddDays(offset))
                .All(checkDate => IsWeekend(checkDate) || festivalDates.Contains(checkDate));

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
        return ranges;
    }

    public async Task<ServiceResult<List<AttendanceDto>>> GetEmployeeTodayFirstClockInAsync(int employeeId)
    {
        if (employeeId <= 0)
            return ServiceResult<List<AttendanceDto>>.Fail("Invalid Employee ID");

        var exist = await _employeeService.GetEmployeeByIdAsync(employeeId);
        if (exist == null || !exist.IsSuccess || exist.Data == null)
            return ServiceResult<List<AttendanceDto>>.Fail("Employee not found");

        var firstClockIn = await _attendanceRepository.GetTodayFirstClockInAsync(employeeId);

        if (firstClockIn == null)
            return ServiceResult<List<AttendanceDto>>.Fail("Attendance not found");

        return ServiceResult<List<AttendanceDto>>.Success(_mapper.Map<List<AttendanceDto>>(firstClockIn));

    }

    private static bool IsWeekend(DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}