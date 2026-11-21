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

    public async Task<ServiceResult<List<LeaveTypeDto>>> GetEmployeeLeaveTypesAsync()
    {
        var entities = await _leaveRepository.GetLeaveTypesAsync();

        var dtos = _mapper.Map<List<LeaveTypeDto>>(entities);

        var today = DateTime.Today;

        int financialYear = today.Month >= 4 ? today.Year : today.Year - 1;

        var firstHalfStart = new DateTime(financialYear, 4, 1);
        var firstHalfEnd = new DateTime(financialYear, 9, 30);

        bool isFirstHalf = today >= firstHalfStart && today <= firstHalfEnd;
        //bool isFirstHalf = true;

        dtos = dtos.Select(leave =>
        {
            // Bug 8 FIX: Use ceiling to avoid truncating leave days (7/2 was 3, now 4)
            if (isFirstHalf)
                leave.MaxPerYear = (int)Math.Ceiling(leave.MaxPerYear / 2.0);

            return leave;
        }).ToList();

        return ServiceResult<List<LeaveTypeDto>>.Success(dtos);
    }


    public async Task<ServiceResult<LeaveTypeDto?>> GetLeaveTypeByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<LeaveTypeDto?>.Fail("Invalid ID");

        var entity = await _leaveRepository.GetLeaveTypeByIdAsync(id);

        if (entity == null)
            return ServiceResult<LeaveTypeDto?>.Fail("Leave not found");

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
            return ServiceResult<bool>.Fail("Leave Type not found");

        var deleted = await _leaveRepository.DeleteLeaveTypeAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Leave type deleted successfully.")
            : ServiceResult<bool>.Fail("Leave Type not found");
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
            return ServiceResult<LeaveRequestDto?>.Fail("Invalid ID");

        var entity = await _leaveRepository.GetLeaveRequestByIdAsync(id);

        if (entity == null)
            return ServiceResult<LeaveRequestDto?>.Fail("Leave request not found");

        return ServiceResult<LeaveRequestDto?>.Success(_mapper.Map<LeaveRequestDto>(entity));
    }

    public async Task<ServiceResult<List<LeaveRequestDto>>> GetLeaveRequestsByEmployeeIdAsync(int employeeId)
    {
        if (employeeId <= 0)
            return ServiceResult<List<LeaveRequestDto>>.Fail("Invalid Employee Id");

        var entities = await _leaveRepository.GetLeaveRequestByEmployeeIdAsync(employeeId);

        if (entities == null || !entities.Any())
            return ServiceResult<List<LeaveRequestDto>>.Fail("Leave requests not found for this employee");

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
            return ServiceResult<bool>.Fail("Leave request not found");

        var deleted = await _leaveRepository.DeleteLeaveRequestAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Leave request deleted successfully.")
            : ServiceResult<bool>.Fail("Leave request not found");
    }

    // =========================================================
    // STATUS & ATTENDANCE LOGIC
    // =========================================================

    public async Task<ServiceResult<bool>> UpdateLeaveRequestStatusAsync(int leaveRequestId, Status status, int approvedBy)
    {
        if (leaveRequestId <= 0)
            return ServiceResult<bool>.Fail("Invalid Leave Request Id"); 

        var updated = await _leaveRepository.UpdateLeaveRequestStatusAsync(leaveRequestId, status, approvedBy);

        if (!updated)
            return ServiceResult<bool>.Fail("Failed to update leave status or status is not approved.");

        if (status == Status.Approved)
            await HandleApprovedLeaveAsync(leaveRequestId);

        return ServiceResult<bool>.Success(true, "Leave status updated successfully.");
    }

    public async Task<ServiceResult<int>> BulkApproveLeaveRequestsAsync(List<int> requestIds, int approvedBy)
    {
        if (requestIds == null || !requestIds.Any() || approvedBy <= 0)
            return ServiceResult<int>.Fail("Invalid request data.");

        var approvedRequests = await _leaveRepository.BulkApproveLeaveRequestsAsync(requestIds, approvedBy);

        if (!approvedRequests.Any())
            return ServiceResult<int>.Fail("No pending leave requests found to approve.");

        // Pure LINQ execution (zero loops)
        await approvedRequests.Aggregate(
            Task.CompletedTask,
            async (prev, lr) =>
            {
                await prev;
                await HandleApprovedLeaveAsync(lr.Id);
            });

        return ServiceResult<int>.Success(approvedRequests.Count, $"Successfully approved {approvedRequests.Count} leave request(s).");
    }

    public async Task<ServiceResult<bool>> CancelLeaveRequestAsync(int id, int employeeId)
    {
        if (id <= 0 || employeeId <= 0)
            return ServiceResult<bool>.Fail("Invalid Id");

        var leave = await _leaveRepository.GetLeaveRequestByIdAsync(id);

        if (leave == null)
            return ServiceResult<bool>.Fail("Leave Request not found");

        if (leave.EmployeeId != employeeId)
            return ServiceResult<bool>.Fail("Unauthorized action.");

        if (leave.StatusId is Status.Approved or Status.Rejected or Status.Cancelled)
            return ServiceResult<bool>.Fail("Cannot cancel this leave request");

        var cancelled = await _leaveRepository.CancelLeaveRequestAsync(id, employeeId);

        return cancelled
            ? ServiceResult<bool>.Success(true, "Leave cancelled successfully.")
            : ServiceResult<bool>.Fail("Failed to cancel leave.");
    }

    public async Task<ServiceResult<bool>> RevokeLeaveRequestAsync(int employeeId, DateTime FromDate, DateTime ToDate)
    {
        var targetStart = FromDate.Date;
        var targetEnd = ToDate.Date;

        var overlappingLeaves = await _leaveRepository.GetApprovedLeavesOverlappingRangeAsync(employeeId, targetStart, targetEnd);

        if (overlappingLeaves != null)
        {
            await overlappingLeaves.Aggregate(Task.CompletedTask, async (prev, leave) =>
            {
                await prev;
                await _leaveRepository.SoftDeleteLeaveRequestAsync(leave);

                if (targetStart > leave.StartDate.Date)
                {
                    await CreateAdjustedLeaveSegmentAsync(leave, leave.StartDate.Date, targetStart.AddDays(-1), ensureAttendance: true);
                }

                if (targetEnd < leave.EndDate.Date)
                {
                    await CreateAdjustedLeaveSegmentAsync(leave, targetEnd.AddDays(1), leave.EndDate.Date, ensureAttendance: true);
                }
            });
        }

        var revokeDates = Enumerable.Range(0, (targetEnd - targetStart).Days + 1)
            .Select(offset => targetStart.AddDays(offset));

        await revokeDates.Aggregate(Task.CompletedTask, async (prev, date) =>
        {
            await prev;
            await _attendanceRepository.DeleteAttendanceAsync(date, employeeId, AttendanceStatus.Leave);
        });

        var response = ServiceResult<bool>.Success(true, "Leave revoked successfully.");
        return response;
    }

    private async Task CreateAdjustedLeaveSegmentAsync(LeaveRequest source, DateTime start, DateTime end, bool ensureAttendance = false)
    {
        var days = await CalculateActualLeaveDaysAsync(start, end);
        if (days <= 0) return;

        var segment = new LeaveRequest
        {
            EmployeeId = source.EmployeeId,
            LeaveTypeId = source.LeaveTypeId,
            StartDate = start,
            EndDate = end,
            LeaveDays = days,
            Reason = source.Reason ?? "Auto Leave Marked",
            StatusId = Status.Approved,
            ApprovalNotes = "Auto Adjusted",
            ApprovedBy = source.ApprovedBy,
            ApprovedDate = source.ApprovedDate ?? DateTime.Now,
            IsActive = true,
            IsDeleted = false,
            CreatedOn = DateTime.Now
        };

        await _leaveRepository.CreateLeaveRequestAsync(segment);

        if (ensureAttendance)
        {
            var segmentDates = Enumerable.Range(0, (end.Date - start.Date).Days + 1)
                .Select(offset => start.Date.AddDays(offset))
                .Where(d => d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday));

            await segmentDates.Aggregate(Task.CompletedTask, async (prev, d) =>
            {
                await prev;
                await _attendanceRepository.EnsureLeaveAttendanceAsync(source.EmployeeId, d, source.Reason);
            });
        }
    }

    private async Task HandleApprovedLeaveAsync(int leaveRequestId)
    {
        var leaveRequest = await _leaveRepository.GetLeaveRequestByIdAsync(leaveRequestId);
        if (leaveRequest == null) return;

        await ReconcileOverlappingAutoLeavesAsync(leaveRequest);

        var festivalDates = await GetFestivalDatesAsync();
        await SynchronizeLeaveAttendancesAsync(leaveRequest, festivalDates);
    }

    private async Task ReconcileOverlappingAutoLeavesAsync(LeaveRequest leaveRequest)
    {
        var overlappingLeaves = await _leaveRepository.GetApprovedLeavesOverlappingRangeAsync(
            leaveRequest.EmployeeId,
            leaveRequest.StartDate.Date,
            leaveRequest.EndDate.Date);

        var autoLeaves = overlappingLeaves
            .Where(lr => lr.Id != leaveRequest.Id &&
                         (lr.Reason == "Auto Leave Marked" ||
                          lr.ApprovalNotes == "Auto Approved" ||
                          lr.ApprovalNotes == "Auto Adjusted"))
            .ToList();

        await autoLeaves.Aggregate(Task.CompletedTask, async (prev, autoLeave) =>
        {
            await prev;
            await _leaveRepository.SoftDeleteLeaveRequestAsync(autoLeave);

            if (autoLeave.StartDate.Date < leaveRequest.StartDate.Date)
            {
                await CreateAdjustedLeaveSegmentAsync(autoLeave, autoLeave.StartDate.Date, leaveRequest.StartDate.Date.AddDays(-1));
            }

            if (autoLeave.EndDate.Date > leaveRequest.EndDate.Date)
            {
                await CreateAdjustedLeaveSegmentAsync(autoLeave, leaveRequest.EndDate.Date.AddDays(1), autoLeave.EndDate.Date);
            }
        });
    }

    private async Task SynchronizeLeaveAttendancesAsync(LeaveRequest leaveRequest, HashSet<DateTime> festivalDates)
    {
        var leaveDates = Enumerable
            .Range(0, (leaveRequest.EndDate.Date - leaveRequest.StartDate.Date).Days + 1)
            .Select(offset => leaveRequest.StartDate.Date.AddDays(offset))
            .Where(date =>
                date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) &&
                !festivalDates.Contains(date));

        await leaveDates.Aggregate(Task.CompletedTask, async (prev, date) =>
        {
            await prev;
            if (date <= DateTime.Now)
            {
                await _attendanceRepository.DeleteAttendanceAsync(date, leaveRequest.EmployeeId);
            }
            await _attendanceRepository.CreateAttendanceAsync(new Attendance
            {
                EmployeeId = leaveRequest.EmployeeId,
                Date = date,
                StatusId = AttendanceStatus.Leave,
                TotalHours = TimeSpan.Zero,
                Notes = leaveRequest.Reason
            });
        });
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
        return Enumerable.Range(0, (endDate.Date - startDate.Date).Days + 1)
            .Select(offset => startDate.Date.AddDays(offset))
            .Count(date => date.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && !festivalDates.Contains(date));
    }
}