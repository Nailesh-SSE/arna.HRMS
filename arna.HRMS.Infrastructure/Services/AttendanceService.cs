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

    public Task<ServiceResult<List<AttendanceDto>>> GetAttendanceByStatusAndEmployeeIdAsync(AttendanceStatus? status, int? employeeId) =>
        GetAttendanceByStatusAndEmployeeIdAsync(status, employeeId, null, null);

    public async Task<ServiceResult<List<AttendanceDto>>> GetAttendanceByStatusAndEmployeeIdAsync(AttendanceStatus? status, int? employeeId, DateTime? fromDate, DateTime? toDate)
    {
        var data = await _attendanceRepository.GetAttendanceByStatusAndEmployeeId(status, employeeId, fromDate, toDate);
        var mapped = _mapper.Map<List<AttendanceDto>>(data);
        var response = ServiceResult<List<AttendanceDto>>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<AttendanceDto?>> GetAttendanceByIdAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<AttendanceDto?>.Fail("Invalid Attendance ID");
            return failId;
        }

        var attendance = await _attendanceRepository.GetAttendanceByIdAsync(id);
        if (attendance == null)
        {
            var failNotFound = ServiceResult<AttendanceDto?>.Fail("Attendance not found");
            return failNotFound;
        }

        var mapped = _mapper.Map<AttendanceDto>(attendance);
        if (mapped == null)
        {
            var failMap = ServiceResult<AttendanceDto?>.Fail("Fail to find Attendance");
            return failMap;
        }

        var response = ServiceResult<AttendanceDto?>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<AttendanceDto>> CreateAttendanceAsync(AttendanceDto attendanceDto)
    {
        if (attendanceDto == null)
        {
            var failReq = ServiceResult<AttendanceDto>.Fail("Invalid request");
            return failReq;
        }

        var validation = await _validator.ValidateCreateAsync(attendanceDto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<AttendanceDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var employeeResult = await _employeeService.GetEmployeeByIdAsync(attendanceDto.EmployeeId);
        if (employeeResult == null || !employeeResult.IsSuccess || employeeResult.Data == null)
        {
            var failEmp = ServiceResult<AttendanceDto>.Fail("Employee not found");
            return failEmp;
        }

        var entity = _mapper.Map<Attendance>(attendanceDto);
        if (entity.DeviceId == null || entity.DeviceId == 0)
        {
            entity.DeviceId = DeviceType.LaptopDesktop;
        }

        await CreateAbsentAndHolidayAsync(entity.EmployeeId, entity.Date, employeeResult.Data.HireDate);

        if (entity.StatusId == AttendanceStatus.Present || entity.ClockIn != null)
        {
            await _leaveService.RevokeLeaveRequestAsync(entity.EmployeeId, entity.Date, entity.Date);
        }

        var created = await _attendanceRepository.CreateAttendanceAsync(entity);
        var mapped = _mapper.Map<AttendanceDto>(created);
        if (mapped == null)
        {
            var failCreate = ServiceResult<AttendanceDto>.Fail("Fail to Create Attendance");
            return failCreate;
        }

        var response = ServiceResult<AttendanceDto>.Success(mapped, "Attendance created successfully.");
        return response;
    }

    public async Task<ServiceResult<List<MonthlyAttendanceDto>>> GetAttendanceByMonthAsync(int year, int month, int? employeeId, DateTime? date, AttendanceStatus? statusId)
    {
        if (year <= 0)
        {
            var failYear = ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid year");
            return failYear;
        }

        if (month < 1 || month > 12)
        {
            var failMonth = ServiceResult<List<MonthlyAttendanceDto>>.Fail("Invalid month");
            return failMonth;
        }

        if (employeeId.HasValue && employeeId > 0)
        {
            var exist = await _employeeService.GetEmployeeByIdAsync(employeeId.Value);
            if (exist == null || !exist.IsSuccess || exist.Data == null)
            {
                var failEmp = ServiceResult<List<MonthlyAttendanceDto>>.Fail("No such Employee Found");
                return failEmp;
            }
        }

        var result = await _attendanceRepository.GetAttendanceByMonthAsync(year, month, employeeId, date, statusId);
        if (result.Any())
        {
            var success = ServiceResult<List<MonthlyAttendanceDto>>.Success(result);
            return success;
        }

        var failNotFound = ServiceResult<List<MonthlyAttendanceDto>>.Fail("Data not found");
        return failNotFound;
    }

    public async Task<ServiceResult<AttendanceDto?>> GetTodayLastEntryAsync(int employeeId)
    {
        if (employeeId <= 0)
        {
            var failId = ServiceResult<AttendanceDto?>.Fail("Invalid Employee ID");
            return failId;
        }

        var exist = await _employeeService.GetEmployeeByIdAsync(employeeId);
        if (exist == null || !exist.IsSuccess || exist.Data == null)
        {
            var failEmp = ServiceResult<AttendanceDto?>.Fail("No such Employee Found");
            return failEmp;
        }

        var last = await _attendanceRepository.GetLastAttendanceTodayAsync(employeeId);
        if (last == null)
        {
            var failAtt = ServiceResult<AttendanceDto?>.Fail("Attendance not found");
            return failAtt;
        }

        var mapped = _mapper.Map<AttendanceDto>(last);
        if (mapped == null)
        {
            var failMap = ServiceResult<AttendanceDto?>.Fail("No data found");
            return failMap;
        }

        var response = ServiceResult<AttendanceDto?>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<List<AttendanceDto>>> GetEmployeeTodayFirstClockInAsync(int employeeId)
    {
        if (employeeId <= 0)
        {
            var failId = ServiceResult<List<AttendanceDto>>.Fail("Invalid Employee ID");
            return failId;
        }

        var exist = await _employeeService.GetEmployeeByIdAsync(employeeId);
        if (exist == null || !exist.IsSuccess || exist.Data == null)
        {
            var failEmp = ServiceResult<List<AttendanceDto>>.Fail("Employee not found");
            return failEmp;
        }

        var firstClockIn = await _attendanceRepository.GetTodayFirstClockInAsync(employeeId);
        if (firstClockIn == null)
        {
            var failAtt = ServiceResult<List<AttendanceDto>>.Fail("Attendance not found");
            return failAtt;
        }

        var mapped = _mapper.Map<List<AttendanceDto>>(firstClockIn);
        var response = ServiceResult<List<AttendanceDto>>.Success(mapped);
        return response;
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

        var festivalDates = await GetFestivalDatesInRangeAsync(effectiveStartDate, newAttendanceDate.Date);
        var leaveDates = await GetEmployeeExistingLeaveDatesInRangeAsync(employeeId, effectiveStartDate, newAttendanceDate.Date);
        var (paidLeaveType, unpaidLeaveType, fallbackLeaveType) = await ResolveAutoLeaveTypesAsync();

        var missingDates = Enumerable
            .Range(1, (newAttendanceDate.Date - effectiveStartDate).Days - 1)
            .Select(i => effectiveStartDate.AddDays(i))
            .Where(date => !IsWeekend(date) && !festivalDates.Contains(date) && !leaveDates.Contains(date))
            .ToList();

        var datesToMarkAsLeave = await MarkMissingAttendancesAsync(employeeId, missingDates);

        if (datesToMarkAsLeave.Any())
        {
            var leaveRequestsResult = await _leaveService.GetLeaveRequestsByFilterAsync(null, employeeId);
            var (paidDates, unpaidDates, fallbackDates) = PartitionLeaveDates(
                datesToMarkAsLeave,
                paidLeaveType,
                unpaidLeaveType,
                fallbackLeaveType,
                leaveRequestsResult?.Data
            );

            await CreateAutoLeaveRequestsAsync(employeeId, paidLeaveType?.Id ?? 0, paidDates, festivalDates);
            await CreateAutoLeaveRequestsAsync(employeeId, unpaidLeaveType?.Id ?? 0, unpaidDates, festivalDates);
            await CreateAutoLeaveRequestsAsync(employeeId, fallbackLeaveType?.Id ?? 0, fallbackDates, festivalDates);
        }
    }

    private async Task<HashSet<DateTime>> GetFestivalDatesInRangeAsync(DateTime startDate, DateTime endDate)
    {
        var festivalHolidaysResult = await _festivalHolidayService.GetFestivalHolidaysAsync();
        return festivalHolidaysResult?.Data?
            .Where(f => f.Date.Date > startDate && f.Date.Date < endDate)
            .Select(f => f.Date.Date)
            .ToHashSet() ?? new HashSet<DateTime>();
    }

    private async Task<HashSet<DateTime>> GetEmployeeExistingLeaveDatesInRangeAsync(int employeeId, DateTime startDate, DateTime endDate)
    {
        var leaveRequestsResult = await _leaveService.GetLeaveRequestsByFilterAsync(null, employeeId);
        return leaveRequestsResult?.Data?
            .Where(l => (l.StatusId == Status.Approved || l.StatusId == Status.Pending) &&
                        l.EndDate.Date > startDate &&
                        l.StartDate.Date < endDate)
            .SelectMany(l => Enumerable.Range(0, (l.EndDate.Date - l.StartDate.Date).Days + 1)
                .Select(offset => l.StartDate.Date.AddDays(offset)))
            .ToHashSet() ?? new HashSet<DateTime>();
    }

    private async Task<(LeaveTypeDto? paid, LeaveTypeDto? unpaid, LeaveTypeDto? fallback)> ResolveAutoLeaveTypesAsync()
    {
        var leaveTypesResult = await _leaveService.GetLeaveTypesAsync();
        var allLeaveTypes = leaveTypesResult?.Data ?? new List<LeaveTypeDto>();
        var activeLeaveTypes = allLeaveTypes.Where(x => x.IsActive).ToList();

        var paidLeaveType = activeLeaveTypes.FirstOrDefault(x => x.LeaveNameId == LeaveName.PaidLeave)
            ?? activeLeaveTypes.FirstOrDefault(x => x.IsPaid);

        var unpaidLeaveType = activeLeaveTypes.FirstOrDefault(x => x.LeaveNameId == LeaveName.UnpaidLeave)
            ?? activeLeaveTypes.FirstOrDefault(x => !x.IsPaid);

        if (paidLeaveType != null && unpaidLeaveType != null && paidLeaveType.Id == unpaidLeaveType.Id)
        {
            unpaidLeaveType = null;
        }

        var fallbackLeaveType = activeLeaveTypes.FirstOrDefault(x => x.LeaveNameId == LeaveName.CasualLeave)
            ?? activeLeaveTypes.FirstOrDefault()
            ?? allLeaveTypes.FirstOrDefault();

        return (paidLeaveType, unpaidLeaveType, fallbackLeaveType);
    }

    private async Task<List<DateTime>> MarkMissingAttendancesAsync(int employeeId, List<DateTime> missingDates)
    {
        var markedDates = new List<DateTime>();
        await missingDates.Aggregate(Task.CompletedTask, async (prev, date) =>
        {
            await prev;
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

                markedDates.Add(date.Date);
            }
        });
        return markedDates;
    }

    private static (List<DateTime> paid, List<DateTime> unpaid, List<DateTime> fallback) PartitionLeaveDates(
        List<DateTime> dates,
        LeaveTypeDto? paidType,
        LeaveTypeDto? unpaidType,
        LeaveTypeDto? fallbackType,
        List<LeaveRequestDto>? existingRequests)
    {
        var paidDates = new List<DateTime>();
        var unpaidDates = new List<DateTime>();
        var fallbackDates = new List<DateTime>();

        if (paidType != null && unpaidType != null)
        {
            var today = DateTime.Today;
            int fyYear = today.Month >= 4 ? today.Year : today.Year - 1;
            var fyStart = new DateTime(fyYear, 4, 1);
            var fyEnd = new DateTime(fyYear + 1, 3, 31);

            int usedPaidDays = existingRequests?
                .Where(l => l.StatusId == Status.Approved &&
                            l.LeaveTypeId == paidType.Id &&
                            l.StartDate >= fyStart &&
                            l.StartDate <= fyEnd)
                .Sum(l => l.LeaveDays) ?? 0;

            int paidQuota = paidType.MaxPerYear;
            int remainingPaidDays = Math.Max(0, paidQuota - usedPaidDays);

            paidDates = dates.Take(remainingPaidDays).ToList();
            unpaidDates = dates.Skip(remainingPaidDays).ToList();
        }
        else if (paidType != null)
        {
            paidDates.AddRange(dates);
        }
        else if (unpaidType != null)
        {
            unpaidDates.AddRange(dates);
        }
        else if (fallbackType != null)
        {
            fallbackDates.AddRange(dates);
        }

        return (paidDates, unpaidDates, fallbackDates);
    }

    private async Task CreateAutoLeaveRequestsAsync(int employeeId, int leaveTypeId, List<DateTime> dates, HashSet<DateTime> festivalDates)
    {
        if (!dates.Any() || leaveTypeId <= 0) return;

        var ranges = GroupDatesIntoRanges(dates, festivalDates);
        await ranges.Aggregate(Task.CompletedTask, async (prev, range) =>
        {
            await prev;
            await _leaveService.CreateLeaveRequestAsync(new LeaveRequestDto
            {
                EmployeeId = employeeId,
                LeaveTypeId = leaveTypeId,
                StartDate = range.Start,
                EndDate = range.End,
                Reason = "Auto Leave Marked",
                StatusId = Status.Approved,
                ApprovalNotes = "Auto Approved",
                ApprovedDate = DateTime.Now.Date
            });
        });
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

    private static bool IsWeekend(DateTime date) =>
        date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;
}
