using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace arna.HRMS.Infrastructure.Services;

public class LeaveService : ILeaveService
{
    private readonly LeaveRepository _leaveRepository;
    private readonly IFestivalHolidayService _festivalHoliday;
    private readonly AttendanceRepository _attendanceService;
    private readonly IMapper _mapper;
    private readonly LeaveValidator _validator;

    public LeaveService(
        LeaveRepository leaveRepository,
        IMapper mapper,
        IFestivalHolidayService festivalHoliday,
        AttendanceRepository attendanceService,
        LeaveValidator validator)
    {
        _leaveRepository = leaveRepository;
        _mapper = mapper;
        _festivalHoliday = festivalHoliday;
        _attendanceService = attendanceService;
        _validator = validator;
    }

    // ============================
    // LEAVE TYPE METHODS
    // ============================

    public async Task<ServiceResult<List<LeaveTypeDto>>> GetLeaveTypeAsync()
    {
        var leave = await _leaveRepository.GetLeaveTypeAsync();
        var list = _mapper.Map<List<LeaveTypeDto>>(leave);

        return ServiceResult<List<LeaveTypeDto>>.Success(list);
    }

    public async Task<ServiceResult<LeaveTypeDto>> GetLeaveTypeByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<LeaveTypeDto>.Fail("Invalid ID");

        var leave = await _leaveRepository.GetLeaveTypeByIdAsync(id);

        if (leave == null)
            return ServiceResult<LeaveTypeDto>.Fail("Leave not found");
        var dto = _mapper.Map<LeaveTypeDto>(leave);
        return dto != null
            ? ServiceResult<LeaveTypeDto>.Success(dto)
            : ServiceResult<LeaveTypeDto>.Fail("Fail to find Leave Type") ;
    }

    public async Task<ServiceResult<LeaveTypeDto>> CreateLeaveTypeAsync(LeaveTypeDto dto)
    {
        var validation = await _validator.ValidateLeaveTypeCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<LeaveTypeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var created = await _leaveRepository.CreateLeaveTypeAsync(_mapper.Map<LeaveType>(dto));
        var data = _mapper.Map<LeaveTypeDto>(created);
        return data != null
            ? ServiceResult<LeaveTypeDto>.Success(data, "Leave type created successfully")
            : ServiceResult<LeaveTypeDto>.Fail("Fail to Create Leave Type");
    }

    public async Task<ServiceResult<LeaveTypeDto>> UpdateLeaveTypeAsync(LeaveTypeDto dto)
    {
        var validation = await _validator.ValidateLeaveTypeUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<LeaveTypeDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var updated = await _leaveRepository.UpdateLeaveTypeAsync(_mapper.Map<LeaveType>(dto));
        if (updated == null)
            return ServiceResult<LeaveTypeDto>.Fail("Leave Type not found");
        var data = _mapper.Map<LeaveTypeDto>(updated);
        return data!=null
            ? ServiceResult<LeaveTypeDto>.Success(data, "Leave type updated successfully")
            : ServiceResult<LeaveTypeDto>.Fail("Fail to Update Leave Type");
    }

    public async Task<ServiceResult<bool>> DeleteLeaveTypeAsync(int id)
    {
        var leave = await GetLeaveTypeByIdAsync(id);
        if (!leave.IsSuccess)
            return ServiceResult<bool>.Fail("Leave Type not found");

        var deleted = await _leaveRepository.DeleteLeaveTypeAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Leave type deleted successfully")
            : ServiceResult<bool>.Fail("Leave Type not found");
    }

    // ============================
    // LEAVE REQUEST METHODS
    // ============================

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestAsync()
    {
        var leave = await _leaveRepository.GetLeaveRequestAsync();
        var list = _mapper.Map<List<LeaveRequestDto>>(leave);

        return ServiceResult<List<LeaveRequestDto>>.Success(list);
    }

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetByFilterAsync(Status? status, int? employeeId)
    {
        var leave = await _leaveRepository.GetLeaveRequestsByFilterAsync(status, employeeId);
        var list = _mapper.Map<List<LeaveRequestDto>>(leave);

        return list.Any()
            ? ServiceResult<List<LeaveRequestDto>>.Success(list)
            : ServiceResult<List<LeaveRequestDto>>.Fail("No Data Found");
    }

    public async Task<ServiceResult<LeaveRequestDto>> GetLeaveRequestByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<LeaveRequestDto>.Fail("Invalid ID");

        var leave = await _leaveRepository.GetLeaveRequestByIdAsync(id);

        if (leave == null)
            return ServiceResult<LeaveRequestDto>.Fail("Leave request not found");
        var dto = _mapper.Map<LeaveRequestDto>(leave);
        return dto != null
            ? ServiceResult<LeaveRequestDto>.Success(dto)
            : ServiceResult<LeaveRequestDto>.Fail("Fail to Find Leave") ;
    }

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestByEmployeeIdAsync(int employeeId)
    {
        if (employeeId <= 0)
            return ServiceResult<List<LeaveRequestDto>>.Fail("Invalid Employee Id");

        var leave = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(employeeId);
        var list = _mapper.Map<List<LeaveRequestDto>>(leave);

        return list.Any()
            ? ServiceResult<List<LeaveRequestDto>>.Success(list)
            : ServiceResult<List<LeaveRequestDto>>.Fail("Leave requests not found for this employee");
    }

    public async Task<ServiceResult<LeaveRequestDto>> CreateLeaveRequestAsync(LeaveRequestDto dto)
    {
        var validation = await _validator.ValidateLeaveRequestCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<LeaveRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var festivalDates = (await _festivalHoliday.GetFestivalHolidayAsync())
            .Data?
            .Select(f => f.Date.Date)
            .ToHashSet()
            ?? new HashSet<DateTime>();

        dto.LeaveDays = CalculateActualLeaveDays(dto.StartDate, dto.EndDate, festivalDates);

        var created = await _leaveRepository.CreateLeaveRequestAsync(_mapper.Map<LeaveRequest>(dto));

        var data = _mapper.Map<LeaveRequestDto>(created);
        return data!=null
            ? ServiceResult<LeaveRequestDto>.Success(data, "Leave created successfully")
            : ServiceResult<LeaveRequestDto>.Fail("Fail to Create Leave");
    }

    public async Task<ServiceResult<LeaveRequestDto>> UpdateLeaveRequestAsync(LeaveRequestDto dto)
    {
        var validation = await _validator.ValidateLeaveRequestUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<LeaveRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var festivalDates = (await _festivalHoliday.GetFestivalHolidayAsync())
            .Data?
            .Select(f => f.Date.Date)
            .ToHashSet()
            ?? new HashSet<DateTime>();

        dto.LeaveDays = CalculateActualLeaveDays(dto.StartDate, dto.EndDate, festivalDates);

        var updated = await _leaveRepository.UpdateLeaveRequestAsync(_mapper.Map<LeaveRequest>(dto));
        if (updated == null)
            return ServiceResult<LeaveRequestDto>.Fail("Leave request not found");
        var data = _mapper.Map<LeaveRequestDto>(updated);
        return data!=null
            ? ServiceResult<LeaveRequestDto>.Success(data, "Leave updated successfully")
            : ServiceResult<LeaveRequestDto>.Fail("Fail to Update Leave");
    }

    public async Task<ServiceResult<bool>> DeleteLeaveRequestAsync(int id)
    {
        var leave = await GetLeaveRequestByIdAsync(id);
        if (!leave.IsSuccess)
            return ServiceResult<bool>.Fail("Leave request not found");

        var deleted = await _leaveRepository.DeleteLeaveRequestAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Leave deleted successfully")
            : ServiceResult<bool>.Fail("Leave not found");
    }

    // ============================
    // STATUS / ATTENDANCE LOGIC
    // ============================

    public async Task<ServiceResult<bool>> UpdateStatusLeaveAsync(int leaveRequestId, Status status, int approvedBy)
    {
        if (leaveRequestId <= 0)
            return ServiceResult<bool>.Fail("Invalid Leave Request Id");

        var updated = await _leaveRepository.UpdateLeaveStatusAsync(leaveRequestId, status, approvedBy);

        if (status == Status.Approved && updated)
        {
            var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(leaveRequestId);

            if (leaveRequest == null)
                return ServiceResult<bool>.Fail("Failed to find Leave Request");

            var festivalDates = (await _festivalHoliday.GetFestivalHolidayAsync())
                .Data?
                .Select(f => f.Date.Date)
                .ToHashSet()
                ?? new HashSet<DateTime>();

            var leaveDates = Enumerable
                .Range(0, (leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).Days + 1)
                .Select(offset => leaveRequest.StartDate.Date.AddDays(offset));

            var workingLeaveDates = leaveDates.Where(date =>
                date.DayOfWeek != DayOfWeek.Saturday &&
                date.DayOfWeek != DayOfWeek.Sunday &&
                !festivalDates.Contains(date));

            foreach (var date in workingLeaveDates)
            {
                var attendance = new AttendanceDto
                {
                    EmployeeId = leaveRequest.EmployeeId,
                    Date = date,
                    WorkingHours = TimeSpan.Zero,
                    StatusId = AttendanceStatus.Leave,
                    Notes = leaveRequest.Reason
                };

                var entity = _mapper.Map<Attendance>(attendance);
                await _attendanceService.CreateAttendanceAsync(entity);
            }
        }

        return updated
            ? ServiceResult<bool>.Success(true, "Leave status updated successfully")
            : ServiceResult<bool>.Fail("Failed to update leave status or status is not approved.");
    }

    public async Task<ServiceResult<bool>> UpdateLeaveRequestStatusCancelAsync(int id, int employeeId)
    {
        if (id <= 0 || employeeId <= 0)
            return ServiceResult<bool>.Fail("Invalid Id");

        var leaveRequest = await GetLeaveRequestByIdAsync(id);
        if (leaveRequest?.Data == null)
            return ServiceResult<bool>.Fail("Leave Request not found");

        if (leaveRequest.Data?.EmployeeId != employeeId)
            return ServiceResult<bool>.Fail("Unauthorized");

        if (leaveRequest.Data.StatusId is Status.Approved or Status.Cancelled or Status.Rejected)
            return ServiceResult<bool>.Fail("Cannot cancel this leave request");

        var updated = await _leaveRepository.UpdateLeaveRequestStatusCancel(id, employeeId);

        return updated
            ? ServiceResult<bool>.Success(true, "Leave cancelled successfully")
            : ServiceResult<bool>.Fail("Not Found");
    }

    // ============================
    // HELPER
    // ============================

    private static int CalculateActualLeaveDays(DateTime startDate, DateTime endDate, HashSet<DateTime> festivalDates)
    {
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
