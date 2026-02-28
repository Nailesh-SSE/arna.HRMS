using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class LeaveService : ILeaveService
{
    private readonly LeaveRepository _leaveRepository;
    private readonly IFestivalHolidayService _festivalHolidayService;
    private readonly AttendanceRepository _attendanceRepository;
    private readonly IMapper _mapper;
    private readonly LeaveValidator _validator;

    public LeaveService(
        LeaveRepository leaveRepository,
        IFestivalHolidayService festivalHolidayService,
        AttendanceRepository attendanceRepository,
        IMapper mapper,
        LeaveValidator validator)
    {
        _leaveRepository = leaveRepository;
        _festivalHolidayService = festivalHolidayService;
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
        _validator = validator;
    }

    // =========================================================
    // LEAVE TYPE METHODS
    // =========================================================

    public async Task<ServiceResult<List<LeaveTypeDto>>> GetLeaveTypesAsync()
    {
        var entities = await _leaveRepository.GetLeaveTypesAsync();

        var dtos = _mapper.Map<List<LeaveTypeDto>>(entities);

        return ServiceResult<List<LeaveTypeDto>>.Success(dtos);
    }

    public async Task<ServiceResult<LeaveTypeDto?>> GetLeaveTypeByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<LeaveTypeDto?>.Fail("Invalid leave type ID.");

        var entity = await _leaveRepository.GetLeaveTypeByIdAsync(id);

        if (entity == null)
            return ServiceResult<LeaveTypeDto?>.Fail("Leave type not found.");

        return ServiceResult<LeaveTypeDto?>.Success(_mapper.Map<LeaveTypeDto>(entity));
    }

    public async Task<ServiceResult<LeaveTypeDto>> CreateLeaveTypeAsync(LeaveTypeDto dto)
    {
        var validation = await _validator.ValidateLeaveTypeCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<LeaveTypeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<LeaveType>(dto);

        var created = await _leaveRepository.CreateLeaveTypeAsync(entity);

        return ServiceResult<LeaveTypeDto>.Success(_mapper.Map<LeaveTypeDto>(created), "Leave type created successfully.");
    }

    public async Task<ServiceResult<LeaveTypeDto>> UpdateLeaveTypeAsync(LeaveTypeDto dto)
    {
        var validation = await _validator.ValidateLeaveTypeUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<LeaveTypeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<LeaveType>(dto);

        var updated = await _leaveRepository.UpdateLeaveTypeAsync(entity);

        if (updated == null)
            return ServiceResult<LeaveTypeDto>.Fail("Leave type not found.");

        return ServiceResult<LeaveTypeDto>.Success(_mapper.Map<LeaveTypeDto>(updated), "Leave type updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteLeaveTypeAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid leave type ID.");

        var deleted = await _leaveRepository.DeleteLeaveTypeAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Leave type deleted successfully.")
            : ServiceResult<bool>.Fail("Leave type not found.");
    }

    // =========================================================
    // LEAVE REQUEST METHODS
    // =========================================================

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestsAsync()
    {
        var entities = await _leaveRepository.GetLeaveRequestsAsync();

        return ServiceResult<List<LeaveRequestDto>>.Success(_mapper.Map<List<LeaveRequestDto>>(entities));
    }

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestsByFilterAsync(Status? status, int? employeeId)
    {
        var entities = await _leaveRepository.GetLeaveRequestsByFilterAsync(status, employeeId);

        return ServiceResult<List<LeaveRequestDto>>.Success(_mapper.Map<List<LeaveRequestDto>>(entities));
    }

    public async Task<ServiceResult<LeaveRequestDto?>> GetLeaveRequestByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<LeaveRequestDto?>.Fail("Invalid leave request ID.");

        var entity = await _leaveRepository.GetLeaveRequestByIdAsync(id);

        if (entity == null)
            return ServiceResult<LeaveRequestDto?>.Fail("Leave request not found.");

        return ServiceResult<LeaveRequestDto?>.Success(_mapper.Map<LeaveRequestDto>(entity));
    }

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
    {
        if (employeeId <= 0)
            return ServiceResult<List<LeaveRequestDto>>.Fail("Invalid employee ID.");

        var entities = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(employeeId);

        return ServiceResult<List<LeaveRequestDto>>.Success(_mapper.Map<List<LeaveRequestDto>>(entities));
    }

    public async Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto dto)
    {
        var validation = await _validator.ValidateLeaveRequestCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<LeaveRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        dto.LeaveDays = await CalculateActualLeaveDaysAsync(dto.StartDate, dto.EndDate);

        var entity = _mapper.Map<LeaveRequest>(dto);

        var created = await _leaveRepository.CreateLeaveRequestAsync(entity);

        return ServiceResult<LeaveRequestDto>.Success(_mapper.Map<LeaveRequestDto>(created), "Leave request created successfully.");
    }

    public async Task<ServiceResult<LeaveRequestDto>> UpdateLeaveRequestAsync(LeaveRequestDto dto)
    {
        var validation = await _validator.ValidateLeaveRequestUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<LeaveRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        dto.LeaveDays = await CalculateActualLeaveDaysAsync(dto.StartDate, dto.EndDate);

        var entity = _mapper.Map<LeaveRequest>(dto);

        var updated = await _leaveRepository.UpdateLeaveRequestAsync(entity);

        if (updated == null)
            return ServiceResult<LeaveRequestDto>.Fail("Leave request not found.");

        return ServiceResult<LeaveRequestDto>.Success(_mapper.Map<LeaveRequestDto>(updated), "Leave request updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteLeaveRequestAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid leave request ID.");

        var deleted = await _leaveRepository.DeleteLeaveRequestAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Leave request deleted successfully.")
            : ServiceResult<bool>.Fail("Leave request not found.");
    }

    // =========================================================
    // STATUS & ATTENDANCE LOGIC
    // =========================================================

    public async Task<ServiceResult<bool>> UpdateLeaveRequestStatusAsync(int leaveRequestId, Status status, int approvedBy)
    {
        if (leaveRequestId <= 0)
            return ServiceResult<bool>.Fail("Invalid leave request ID."); 

        var updated = await _leaveRepository.UpdateLeaveRequestStatusAsync(leaveRequestId, status, approvedBy);

        if (!updated)
            return ServiceResult<bool>.Fail("Failed to update leave status.");

        if (status == Status.Approved)
            await HandleApprovedLeaveAsync(leaveRequestId);

        return ServiceResult<bool>.Success(true, "Leave status updated successfully.");
    }

    public async Task<ServiceResult<bool>> CancelLeaveRequestAsync(int id, int employeeId)
    {
        if (id <= 0 || employeeId <= 0)
            return ServiceResult<bool>.Fail("Invalid request.");

        var leave = await _leaveRepository.GetLeaveRequestByIdAsync(id);

        if (leave == null)
            return ServiceResult<bool>.Fail("Leave request not found.");

        if (leave.EmployeeId != employeeId)
            return ServiceResult<bool>.Fail("Unauthorized action.");

        if (leave.StatusId is Status.Approved or Status.Rejected or Status.Cancelled)
            return ServiceResult<bool>.Fail("Leave cannot be cancelled.");

        var cancelled = await _leaveRepository.CancelLeaveRequestAsync(id, employeeId);

        return cancelled
            ? ServiceResult<bool>.Success(true, "Leave cancelled successfully.")
            : ServiceResult<bool>.Fail("Failed to cancel leave.");
    }

    // =========================================================
    // PRIVATE HELPERS
    // =========================================================

    private async Task HandleApprovedLeaveAsync(int leaveRequestId)
    {
        var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(leaveRequestId);
        if (leaveRequest == null) return;

        var festivalDates = await GetFestivalDatesAsync();

        var leaveDates = Enumerable
            .Range(0, (leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).Days + 1)
            .Select(offset => leaveRequest.StartDate.Date.AddDays(offset))
            .Where(date =>
                date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) &&
                !festivalDates.Contains(date));

        foreach (var date in leaveDates)
        {
            await _attendanceRepository.CreateAttendanceAsync(new Attendance
            {
                EmployeeId = leaveRequest.EmployeeId,
                Date = date,
                StatusId = AttendanceStatus.Leave,
                TotalHours = TimeSpan.Zero,
                Notes = leaveRequest.Reason
            });
        }
    }

    private async Task<HashSet<DateTime>> GetFestivalDatesAsync()
    {
        var result = await _festivalHolidayService.GetFestivalHolidaysAsync();

        return result.Data?
            .Select(f => f.Date.Date)
            .ToHashSet()
            ?? new HashSet<DateTime>();
    }

    private async Task<int> CalculateActualLeaveDaysAsync(DateTime startDate, DateTime endDate)
    {
        var festivalDates = await GetFestivalDatesAsync();
        int leaveDays = 0;

        for (var date = startDate.Date; date <= endDate.Date; date = date.AddDays(1))
        {
            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                continue;

            if (festivalDates.Contains(date))
                continue;

            leaveDays++;
        }

        return leaveDays;
    }
}