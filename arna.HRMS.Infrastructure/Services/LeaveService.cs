using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class LeaveService : ILeaveService
{
    private readonly LeaveRepository _leaveRepository;
    private readonly IFestivalHolidayService _festivalHoliday;
    private readonly IMapper _mapper;

    public LeaveService(LeaveRepository LeaveRepository, IMapper mapper, IFestivalHolidayService festivalHoliday)
    {
        _leaveRepository = LeaveRepository;
        _mapper = mapper;
        _festivalHoliday = festivalHoliday;
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
        var leave = await _leaveRepository.GetLeaveMasterByIdAsync(id);
        var data = leave == null ? new LeaveMasterDto() : _mapper.Map<LeaveMasterDto>(leave);
        return ServiceResult<LeaveMasterDto>.Success(data);
    }
    public async Task<ServiceResult<LeaveMasterDto>> CreateLeaveMasterAsync(LeaveMasterDto LeaveMasterDto)
    {
        var leave = _mapper.Map<LeaveMaster>(LeaveMasterDto);
        var createdLeaveMaster = await _leaveRepository.CreateLeaveMasterAsync(leave);
        var Data= _mapper.Map<LeaveMasterDto>(createdLeaveMaster);
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

    public async Task<ServiceResult<LeaveRequestDto>> GetLeaveRequestByIdAsync(int id)
    {
        var leave = await _leaveRepository.GetLeaveRequestByIdAsync(id);
        var Data= leave == null ? new LeaveRequestDto() : _mapper.Map<LeaveRequestDto>(leave);
        return ServiceResult<LeaveRequestDto>.Success(Data);
    }

    public async Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto LeaveRequestDto)
    {
        var Festival = _mapper.Map<LeaveRequest>(LeaveRequestDto);
        var createdLeaveRequest = await _leaveRepository.CreateLeaveRequestAsync(Festival);
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

    public async Task<ServiceResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, LeaveStatus status, int approvedBy)
    {
        var updated = await _leaveRepository
            .UpdateLeaveStatusAsync(leaveRequestId, status, approvedBy);

        if (!updated || status != LeaveStatus.Approved)
            return ServiceResult<bool>.Fail("Failed to update leave status or status is not approved.");

        var leaveRequest = await _leaveRepository
            .GetLeaveRequestByIdAsync(leaveRequestId);

        if (leaveRequest == null)
            return ServiceResult<bool>.Fail("Failed to find Leave Request");

        var latestBalance = await _leaveRepository
            .GetLatestByEmployeeAndLeaveTypeAsync(
                leaveRequest.EmployeeId,
                leaveRequest.LeaveTypeId);

        var festivalHolidays = (await _festivalHoliday.GetFestivalHolidayAsync())
            .Data?
            .Select(f => f.Date.Date)
            .ToHashSet()
            ?? new HashSet<DateTime>();

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
