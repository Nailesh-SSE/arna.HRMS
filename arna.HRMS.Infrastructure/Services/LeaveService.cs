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
    private readonly IMapper _mapper;

    public LeaveService(LeaveRepository LeaveRepository, IMapper mapper)
    {
        _leaveRepository = LeaveRepository;
        _mapper = mapper;
    }

    //Leave Master Methods
    public async Task<List<LeaveMasterDto>> GetLeaveMasterAsync()
    {
        var leave = await _leaveRepository.GetLeaveMasterAsync();
        return _mapper.Map<List<LeaveMasterDto>>(leave);
    }
    public async Task<LeaveMasterDto> GetLeaveMasterByIdAsync(int id)
    {
        var leave = await _leaveRepository.GetLeaveMasterByIdAsync(id);
        return leave == null ? new LeaveMasterDto() : _mapper.Map<LeaveMasterDto>(leave);
    }
    public async Task<LeaveMasterDto> CreateLeaveMasterAsync(LeaveMasterDto LeaveMasterDto)
    {
        var leave = _mapper.Map<LeaveMaster>(LeaveMasterDto);
        var createdLeaveMaster = await _leaveRepository.CreateLeaveMasterAsync(leave);
        return _mapper.Map<LeaveMasterDto>(createdLeaveMaster);
    }
    public async Task<bool> DeleteLeaveMasterAsync(int id)
    {
        return await _leaveRepository.DeleteLeaveMasterAsync(id);
    }

    public async Task<LeaveMasterDto> UpdateLeaveMasterAsync(LeaveMasterDto LeaveMasterDto)
    {
        var leave = _mapper.Map<LeaveMaster>(LeaveMasterDto);
        var updatedLeaveMaster = await _leaveRepository.UpdateLeaveMasterAsync(leave);
        return _mapper.Map<LeaveMasterDto>(updatedLeaveMaster);
    }
    public async Task<bool> LeaveExistsAsync(string Name)
    {
        return await _leaveRepository.LeaveExistsAsync(Name);
    }

    //Leave Request Methods
    public async Task<List<LeaveRequestDto>> GetLeaveRequestAsync()
    {
        var leave = await _leaveRepository.GetLeaveRequestAsync();
        return _mapper.Map<List<LeaveRequestDto>>(leave);
    }

    public async Task<LeaveRequestDto> GetLeaveRequestByIdAsync(int id)
    {
        var leave = await _leaveRepository.GetLeaveRequestByIdAsync(id);
        return leave == null ? new LeaveRequestDto() : _mapper.Map<LeaveRequestDto>(leave);
    }

    public async Task<LeaveRequestDto> CreateLeaveRequestAsync(LeaveRequestDto LeaveRequestDto)
    {
        var Festival = _mapper.Map<LeaveRequest>(LeaveRequestDto);
        var createdLeaveRequest = await _leaveRepository.CreateLeaveRequestAsync(Festival);
        return _mapper.Map<LeaveRequestDto>(createdLeaveRequest);
    }
    public async Task<bool> DeleteLeaveRequestAsync(int id)
    {
        return await _leaveRepository.DeleteLeaveRequestAsync(id);
    }

    public async Task<LeaveRequestDto> UpdateLeaveRequestAsync(LeaveRequestDto LeaveRequestDto)
    {
        var Festival = _mapper.Map<LeaveRequest>(LeaveRequestDto);
        var updatedLeaveRequest = await _leaveRepository.UpdateLeaveRequestAsync(Festival);
        return _mapper.Map<LeaveRequestDto>(updatedLeaveRequest);
    }

    public async Task<bool> UpdateStatusLeaveAsync(int leaveRequestId, LeaveStatus status, int approvedBy)
    {
        var updated = await _leaveRepository
            .UpdateLeaveStatusAsync(leaveRequestId, status, approvedBy);

        if (!updated || status != LeaveStatus.Approved)
            return false;

        var leaveRequest = await _leaveRepository
            .GetLeaveRequestByIdAsync(leaveRequestId);

        if (leaveRequest == null)
            return false;

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
        var newUsedLeaves =
            latestBalance.UsedLeaves + leaveRequest.TotalDays;

        var newRemainingLeaves =
            latestBalance.TotalLeaves - newUsedLeaves;

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

        await _leaveRepository
            .CreateLeaveBalanceAsync(newBalance);

        return true;
    }

    //Leave Balance Methods
    public async Task<List<EmployeeLeaveBalanceDto>> GetLeaveBalanceAsync()
    {
        var holiday = await _leaveRepository.GetLeaveBalanceAsync();
        return _mapper.Map<List<EmployeeLeaveBalanceDto>>(holiday);
    }
    
    public async Task<bool> DeleteLeaveBalanceAsync(int id)
    {
        return await _leaveRepository.DeleteLeaveBalanceAsync(id);
    }

    public async Task<EmployeeLeaveBalanceDto> UpdateLeaveBalanceAsync(EmployeeLeaveBalanceDto LeaveBalanceDto)
    {
        var leave = _mapper.Map<EmployeeLeaveBalance>(LeaveBalanceDto);
        var updatedLeaveBalance = await _leaveRepository.UpdateLeaveBalanceAsync(leave);
        return _mapper.Map<EmployeeLeaveBalanceDto>(updatedLeaveBalance);
    }

    public async Task<EmployeeLeaveBalanceDto?> GetLeaveBalanceByEmployeeIdAsync(int employeeId)
    {
        var leaveBalance = await _leaveRepository.GetLeaveBalanceByEmployeeAsync(employeeId);
        return _mapper.Map<EmployeeLeaveBalanceDto>(leaveBalance);
    }
}
