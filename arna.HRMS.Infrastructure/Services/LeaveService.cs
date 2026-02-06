using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class LeaveService : ILeaveService
{
    private readonly LeaveRepository _leaveRepository;
    private readonly IFestivalHolidayService _festivalHoliday;
    private readonly AttendanceRepository _attendanceService;
    private readonly IMapper _mapper;

    public LeaveService(LeaveRepository LeaveRepository, IMapper mapper, IFestivalHolidayService festivalHoliday, AttendanceRepository attendanceService)
    {
        _leaveRepository = LeaveRepository;
        _mapper = mapper;
        _festivalHoliday = festivalHoliday;
        _attendanceService = attendanceService;
    }

    //Leave Master Methods
    public async Task<ServiceResult<List<LeaveMasterDto>>> GetLeaveMasterAsync()
    {
        var leave = await _leaveRepository.GetLeaveMasterAsync();
        var list = _mapper.Map<List<LeaveMasterDto>>(leave);
        return ServiceResult<List<LeaveMasterDto>>.Success(list);
    }
    public async Task<ServiceResult<LeaveMasterDto>> GetLeaveMasterByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<LeaveMasterDto>.Fail("Invalid ID");

        var leave = await _leaveRepository.GetLeaveMasterByIdAsync(id);
        if (leave == null)
            return ServiceResult<LeaveMasterDto>.Success(null, "leave not found");

        var data = leave == null ? new LeaveMasterDto() : _mapper.Map<LeaveMasterDto>(leave);
        return ServiceResult<LeaveMasterDto>.Success(data);
    }
    public async Task<ServiceResult<LeaveMasterDto>> CreateLeaveMasterAsync(LeaveMasterDto LeaveMasterDto)
    {
        if (LeaveMasterDto == null)
            return ServiceResult<LeaveMasterDto>.Fail("Data not Found");
        
        if (string.IsNullOrWhiteSpace(LeaveMasterDto.LeaveName))
            return ServiceResult<LeaveMasterDto>.Fail("Leave name is required");

        if (LeaveMasterDto.MaxPerYear <= 0)
            return ServiceResult<LeaveMasterDto>.Fail("number of days is required");

        var existingLeaves = await _leaveRepository.GetLeaveMasterByNameAsync(LeaveMasterDto.LeaveName);
        var duplicate = existingLeaves.Any(x =>
            x != null &&
            x.IsPaid == LeaveMasterDto.IsPaid
        );

        if (duplicate)
        {
            return ServiceResult<LeaveMasterDto>.Fail(
                $"Leave '{LeaveMasterDto.LeaveName}' with IsPaid = {LeaveMasterDto.IsPaid} already exists"
            );
        }

        var leave = _mapper.Map<LeaveMaster>(LeaveMasterDto);
        var createdLeaveMaster = await _leaveRepository.CreateLeaveMasterAsync(leave);
        var Data = _mapper.Map<LeaveMasterDto>(createdLeaveMaster);
        return ServiceResult<LeaveMasterDto>.Success(Data);
    }
    public async Task<ServiceResult<bool>> DeleteLeaveMasterAsync(int id)
    {
        var Data = await _leaveRepository.DeleteLeaveMasterAsync(id);
        return ServiceResult<bool>.Success(Data);
    }

    public async Task<ServiceResult<LeaveMasterDto>> UpdateLeaveMasterAsync(LeaveMasterDto LeaveMasterDto)
    {
        var leave = _mapper.Map<LeaveMaster>(LeaveMasterDto);
        var updatedLeaveMaster = await _leaveRepository.UpdateLeaveMasterAsync(leave);
        var Data = _mapper.Map<LeaveMasterDto>(updatedLeaveMaster);
        return ServiceResult<LeaveMasterDto>.Success(Data);
    }
    public async Task<ServiceResult<bool>> LeaveExistsAsync(string Name)
    {
        var Data = await _leaveRepository.LeaveExistsAsync(Name);
        return ServiceResult<bool>.Success(Data);
    }

    //Leave Request Methods
    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestAsync()
    {
        var leave = await _leaveRepository.GetLeaveRequestAsync();
        var list = _mapper.Map<List<LeaveRequestDto>>(leave);
        return ServiceResult<List<LeaveRequestDto>>.Success(list);
    }

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetPendingLeaveRequest()
    {
        var leave = await _leaveRepository.GetPendingLeaveRequest();
        var list = _mapper.Map<List<LeaveRequestDto>>(leave);
        return ServiceResult<List<LeaveRequestDto>>.Success(list);
    }

    public async Task<ServiceResult<LeaveRequestDto>> GetLeaveRequestByIdAsync(int id)
    {
        var leave = await _leaveRepository.GetLeaveRequestByIdAsync(id);
        var Data = leave == null ? new LeaveRequestDto() : _mapper.Map<LeaveRequestDto>(leave);
        return ServiceResult<LeaveRequestDto>.Success(Data);
    }

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestByEmployeeIdAsync(int employeeId)
    {
        var leave = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(employeeId);
        var Data = _mapper.Map<List<LeaveRequestDto>>(leave);
        return ServiceResult<List<LeaveRequestDto>>.Success(Data);
    }

    public async Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto LeaveRequestDto)
    {
        var balance = await _leaveRepository.GetLeaveBalanceByEmployeeAsync(LeaveRequestDto.EmployeeId);
        var festivalDates = (await _festivalHoliday.GetFestivalHolidayAsync())
                .Data?
                .Select(f => f.Date.Date)
                .ToHashSet()
                ?? new HashSet<DateTime>();

        int actualLeaveDays = CalculateActualLeaveDays(
            LeaveRequestDto.StartDate,
            LeaveRequestDto.EndDate,
            festivalDates);

        var hasInsufficientBalance = balance.Any(w =>
         w.LeaveMasterId == LeaveRequestDto.LeaveTypeId &&
         w.RemainingLeaves < actualLeaveDays
         );

        if (hasInsufficientBalance)
        {
            return ServiceResult<LeaveRequestDto>
                .Fail("Insufficient leave balance");
        }

        var leave = _mapper.Map<LeaveRequest>(LeaveRequestDto);
        var createdLeaveRequest = await _leaveRepository.CreateLeaveRequestAsync(leave);
        var Data = _mapper.Map<LeaveRequestDto>(createdLeaveRequest);
        return ServiceResult<LeaveRequestDto>.Success(Data);
    }
    public async Task<ServiceResult<bool>> DeleteLeaveRequestAsync(int id)
    {
        var Data = await _leaveRepository.DeleteLeaveRequestAsync(id);
        return ServiceResult<bool>.Success(Data);
    }

    public async Task<ServiceResult<LeaveRequestDto>> UpdateLeaveRequestAsync(LeaveRequestDto LeaveRequestDto)
    {
        var Festival = _mapper.Map<LeaveRequest>(LeaveRequestDto);
        var updatedLeaveRequest = await _leaveRepository.UpdateLeaveRequestAsync(Festival);
        var Data = _mapper.Map<LeaveRequestDto>(updatedLeaveRequest);
        return ServiceResult<LeaveRequestDto>.Success(Data);
    }

    public async Task<ServiceResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status, int approvedBy)
    {
        if (leaveRequestId <= 0)
        {
            return ServiceResult<bool>.Fail("Invalid Leave Request Id");
        }

        var updated = await _leaveRepository.UpdateLeaveStatusAsync(leaveRequestId, status, approvedBy);
        
        if (status == Status.Approved)
        {
            if (!updated || status != Status.Approved)
                return ServiceResult<bool>.Fail("Failed to update leave status or status is not approved.");

            var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(leaveRequestId);

            if (leaveRequest == null)
                return ServiceResult<bool>.Fail("Failed to find Leave Request");

            var latestBalance = await _leaveRepository
                .GetLatestByEmployeeAndLeaveTypeAsync(
                    leaveRequest.EmployeeId,
                    leaveRequest.LeaveTypeId);

            if (latestBalance == null)
            {
                var leaveMaster = await _leaveRepository.GetLeaveMasterByIdAsync(leaveRequest.LeaveTypeId);
                latestBalance = new EmployeeLeaveBalance
                {
                    EmployeeId = leaveRequest.EmployeeId,
                    LeaveMasterId = leaveRequest.LeaveTypeId,
                    TotalLeaves = leaveMaster.MaxPerYear,
                    UsedLeaves = 0,
                    RemainingLeaves = leaveMaster.MaxPerYear,
                    Year = DateTime.Now.Year,
                };
            }

            var festivalDates = (await _festivalHoliday.GetFestivalHolidayAsync())
                .Data?
                .Select(f => f.Date.Date)
                .ToHashSet()
                ?? new HashSet<DateTime>();

            int actualLeaveDays = CalculateActualLeaveDays(
                leaveRequest.StartDate,
                leaveRequest.EndDate,
                festivalDates);

            var newUsedLeaves = latestBalance.UsedLeaves + actualLeaveDays;
            var newRemainingLeaves = latestBalance.TotalLeaves - newUsedLeaves;


            var newBalance = new EmployeeLeaveBalance
            {
                EmployeeId = leaveRequest.EmployeeId,
                LeaveMasterId = leaveRequest.LeaveTypeId,
                TotalLeaves = latestBalance.TotalLeaves,
                UsedLeaves = newUsedLeaves,
                RemainingLeaves = newRemainingLeaves,
                IsActive = true,
                IsDeleted = false,
                CreatedOn = DateTime.Now
            };

            await _leaveRepository.CreateLeaveBalanceAsync(newBalance);

            var leaveDates = Enumerable
                .Range(0, (leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).Days + 1)
                .Select(offset => leaveRequest.StartDate.Date.AddDays(offset));

            var workingLeaveDates = leaveDates
                 .Where(date =>
                     date.DayOfWeek != DayOfWeek.Saturday &&
                     date.DayOfWeek != DayOfWeek.Sunday &&
                     !festivalDates.Contains(date)
                 );

            foreach (var date in workingLeaveDates)
            {
                var attendance = new AttendanceDto
                {
                    EmployeeId = leaveRequest.EmployeeId,
                    Date = date,
                    ClockInTime = null,
                    ClockOutTime = null,
                    WorkingHours = TimeSpan.Zero,
                    Status = AttendanceStatus.Leave,
                    Notes = leaveRequest.Reason
                };
                var dto = _mapper.Map<Attendance>(attendance);
                await _attendanceService.CreateAttendanceAsync(dto);
            }
        }
        return ServiceResult<bool>.Success(true);
    }

    private static int CalculateActualLeaveDays(DateTime startDate, DateTime endDate, HashSet<DateTime> festivalDates)
    {
        int leaveDays = 0;

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            // Skip weekends
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            // Skip festival holidays
            if (festivalDates.Contains(date))
                continue;

            leaveDays++;
        }

        return leaveDays;
    }

    public async Task<ServiceResult<bool>> UpdateLeaveRequestStatusCancelAsync(int id, int employeeId)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid leave ID");
        var updated = await _leaveRepository.UpdateLeaveRequestStatusCancel(id, employeeId);
        return ServiceResult<bool>.Success(updated);
    }

    //Leave Balance Methods
    public async Task<ServiceResult<List<EmployeeLeaveBalanceDto>>> GetLeaveBalanceAsync()
    {
        var holiday = await _leaveRepository.GetLeaveBalanceAsync();
        var Data = _mapper.Map<List<EmployeeLeaveBalanceDto>>(holiday);
        return ServiceResult<List<EmployeeLeaveBalanceDto>>.Success(Data);
    }

    public async Task<ServiceResult<bool>> DeleteLeaveBalanceAsync(int id)
    {
        var Data = await _leaveRepository.DeleteLeaveBalanceAsync(id);
        return ServiceResult<bool>.Success(Data);
    }

    public async Task<ServiceResult<EmployeeLeaveBalanceDto>> UpdateLeaveBalanceAsync(EmployeeLeaveBalanceDto LeaveBalanceDto)
    {
        if(LeaveBalanceDto == null || LeaveBalanceDto.Id <=0 || LeaveBalanceDto.EmployeeId<=0)
        {
            return ServiceResult<EmployeeLeaveBalanceDto>.Fail("Invalid Leave Balance Data");
        }
        if(LeaveBalanceDto.TotalLeaves < 0 || LeaveBalanceDto.UsedLeaves < 0 || LeaveBalanceDto.RemainingLeaves < 0)
        {
            return ServiceResult<EmployeeLeaveBalanceDto>.Fail("Leave counts cannot be negative");
        }
        var leave = _mapper.Map<EmployeeLeaveBalance>(LeaveBalanceDto);
        var updatedLeaveBalance = await _leaveRepository.UpdateLeaveBalanceAsync(leave);
        var Data = _mapper.Map<EmployeeLeaveBalanceDto>(updatedLeaveBalance);
        return ServiceResult<EmployeeLeaveBalanceDto>.Success(Data);
    }

    public async Task<ServiceResult<List<EmployeeLeaveBalanceDto?>>> GetLeaveBalanceByEmployeeIdAsync(int employeeId)
    {
        var leaveBalance = await _leaveRepository.GetLeaveBalanceByEmployeeAsync(employeeId);
        var Data = _mapper.Map<List<EmployeeLeaveBalanceDto?>>(leaveBalance);
        return ServiceResult<List<EmployeeLeaveBalanceDto?>>.Success(Data);
    }
}
