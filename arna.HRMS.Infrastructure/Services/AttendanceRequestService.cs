using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly AttendanceRequestRepository _repository;
    private readonly AttendanceRepository _attendanceRepository;
    private readonly IMapper _mapper;
    private readonly IAttendanceService _attendanceService;
    private readonly ILeaveService _leaveService;
    private readonly IEmployeeService _employeeService;
    private readonly AttendanceRequestValidator _validator;

    public AttendanceRequestService(
        AttendanceRequestRepository repository,
        AttendanceRepository attendanceRepository,
        IMapper mapper,
        IAttendanceService attendanceService,
        ILeaveService leaveService,
        IEmployeeService employeeService,
        AttendanceRequestValidator validator)
    {
        _repository = repository;
        _attendanceRepository = attendanceRepository;
        _mapper = mapper;
        _attendanceService = attendanceService;
        _leaveService = leaveService;
        _employeeService = employeeService;
        _validator = validator;
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequestsAsync(int? employeeId, Status? status)
    {
        var data = await _repository.GetAttendanceRequests(employeeId, status);

        return ServiceResult<List<AttendanceRequestDto>>.Success(_mapper.Map<List<AttendanceRequestDto>>(data));
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetPendingAttendanceRequestsAsync()
    {
        var data = await _repository.GetPendingAttendanceRequests();

        return ServiceResult<List<AttendanceRequestDto>>.Success(_mapper.Map<List<AttendanceRequestDto>>(data));
    }

    public async Task<ServiceResult<AttendanceRequestDto?>> GetAttendanceRequestByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<AttendanceRequestDto?>.Fail("Invalid attendance request ID.");

        var entity = await _repository.GetAttendanceRequestByIdAsync(id);

        if (entity == null)
            return ServiceResult<AttendanceRequestDto?>.Fail("Attendance request not found.");

        return ServiceResult<AttendanceRequestDto?>.Success(_mapper.Map<AttendanceRequestDto>(entity));
    }

    public async Task<ServiceResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<AttendanceRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<AttendanceRequest>(dto);

        var created = await _repository.CreateAttendanceRequestAsync(entity);

        return ServiceResult<AttendanceRequestDto>.Success(_mapper.Map<AttendanceRequestDto>(created), "Attendance request created successfully.");
    }

    public async Task<ServiceResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(AttendanceRequestDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);

        if (!validation.IsValid)
            return ServiceResult<AttendanceRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var entity = _mapper.Map<AttendanceRequest>(dto);

        var updated = await _repository.UpdateAttendanceRequestAsync(entity);

        return ServiceResult<AttendanceRequestDto>.Success(_mapper.Map<AttendanceRequestDto>(updated), "Attendance request updated successfully.");
    }

    public async Task<ServiceResult<bool>> DeleteAttendanceRequestAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid attendance request ID.");

        var deleted = await _repository.DeleteAttendanceRequestAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Attendance request deleted successfully.")
            : ServiceResult<bool>.Fail("Attendance request not found.");
    }

    public async Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id, Status status, int approvedBy)
    {
        var validation = await _validator.ValidateStatusUpdateAsync(id, status, approvedBy);

        if (!validation.IsValid)
            return ServiceResult<bool>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var updated = await _repository.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);

        if (!updated)
            return ServiceResult<bool>.Fail("Failed to update attendance request status.");

        if (status == Status.Approved)
        {
            var request = await _repository.GetAttendanceRequestByIdAsync(id);

            if (request != null)
                await CreateAttendanceFromRequestAsync(_mapper.Map<AttendanceRequestDto>(request));

            await RecalculateAutoGeneratedLeavesAsync(request.EmployeeId, request.FromDate);
        }

        return ServiceResult<bool>.Success(true, "Attendance request status updated successfully.");
    }

    public async Task<ServiceResult<bool>> CancelAttendanceRequestAsync(int id, int employeeId)
    {
        if (id <= 0 || employeeId <= 0)
            return ServiceResult<bool>.Fail("Invalid request.");

        var cancelled = await _repository.CancelAttendanceRequestAsync(id, employeeId);

        return cancelled
            ? ServiceResult<bool>.Success(true, "Attendance request cancelled successfully.")
            : ServiceResult<bool>.Fail("Failed to cancel attendance request.");
    }

    private async Task CreateAttendanceFromRequestAsync(AttendanceRequestDto req)
    {
        var fromDate = req.FromDate!.Value.Date;
        var toDate = req.ToDate!.Value.Date;

        var clockIn = req.ClockIn!.Value.TimeOfDay;
        var clockOut = req.ClockOut!.Value.TimeOfDay;

        if (fromDate == toDate)
        {
            await InsertAttendanceAsync(req, fromDate, clockIn, null, TimeSpan.Zero);
            await InsertAttendanceAsync(req, fromDate, null, clockOut, req.TotalHours);
            return;
        }

        await InsertAttendanceAsync(req, fromDate, clockIn, null, TimeSpan.Zero);
        await InsertAttendanceAsync(req, toDate, null, clockOut, clockOut);
    }

    private async Task InsertAttendanceAsync(AttendanceRequestDto req, DateTime date, TimeSpan? clockIn, TimeSpan? clockOut, TimeSpan totalHours)
    {
        await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = req.EmployeeId,
            Date = date,
            ClockInTime = clockIn,
            ClockOutTime = clockOut,
            WorkingHours = totalHours,
            StatusId = AttendanceStatus.Present,
            Notes = req.Description ?? string.Empty
        });
    }
    private async Task RecalculateAutoGeneratedLeavesAsync(int employeeId, DateTime affectedDate)
    {
        // Get all auto-generated approved leaves
        var autoLeaves = (await _leaveService
            .GetLeaveRequestsAsync())
            .Data?
            .Where(l =>
                l.EmployeeId == employeeId &&
                l.StatusId == Status.Approved &&
                l.Reason == "Auto-generated leave for absent days" &&
                affectedDate.Date >= l.StartDate.Date &&
                affectedDate.Date <= l.EndDate.Date)
            .ToList();

        if (autoLeaves == null || !autoLeaves.Any())
            return;

        foreach (var leave in autoLeaves)
        {
            // Remove old leave request
            await _leaveService.DeleteLeaveRequestAsync(leave.Id);

            // Remove old leave attendance
            var leaveDates = Enumerable.Range(
                    0,
                    (leave.EndDate.Date - leave.StartDate.Date).Days + 1)
                .Select(offset =>
                    leave.StartDate.Date.AddDays(offset));

            foreach (var date in leaveDates)
            {
                // Skip corrected attendance date
                if (date.Date == affectedDate.Date)
                    continue;

                var attendance =
                    await _attendanceRepository
                        .GetAttendanceByEmployeeAndDateAsync(
                            employeeId,
                            date);

                if (attendance != null &&
                    attendance.StatusId == AttendanceStatus.Leave &&
                    attendance.Notes == "Auto-generated leave")
                {
                    await _attendanceRepository
                        .DeleteAttendanceAsync(date, employeeId);
                }
            }

            // Rebuild remaining leave dates
            var remainingDates = leaveDates
                .Where(d => d.Date != affectedDate.Date)
                .ToList();

            if (!remainingDates.Any())
                continue;

            // Group again
            var groupedDates = remainingDates
                .Select((date, index) => new { date, index })
                .GroupBy(x => x.date.AddDays(-x.index));

            var leaveType =
                await _leaveService.GetLeaveTypesAsync();

            var employee =
                await _employeeService
                    .GetEmployeeByIdAsync(employeeId);

            foreach (var group in groupedDates)
            {
                var dates = group
                    .Select(x => x.date)
                    .OrderBy(d => d)
                    .ToList();

                // Recreate attendance
                foreach (var date in dates)
                {
                    await _attendanceRepository
                        .CreateAttendanceAsync(new Attendance
                        {
                            EmployeeId = employeeId,
                            Date = date,
                            ClockIn = null,
                            ClockOut = null,
                            TotalHours = TimeSpan.Zero,
                            StatusId = AttendanceStatus.Leave,
                            Notes = "Auto-generated leave",
                            CreatedOn = DateTime.UtcNow
                        });
                }

                // Recreate leave request
                await _leaveService
                    .CreateLeaveRequestAsync(
                        new LeaveRequestDto
                        {
                            EmployeeId = employeeId,
                            EmployeeName = employee.Data?.FullName,
                            EmployeeNumber = employee.Data?.EmployeeNumber,
                            LeaveTypeId = leaveType.Data?.FirstOrDefault()?.Id ?? 0,
                            LeaveTypeName = leaveType.Data?.FirstOrDefault()?.Description,
                            StartDate = dates.First(),
                            EndDate = dates.Last(),
                            LeaveDays = dates.Count,
                            Reason = "Auto-generated leave for absent days",
                            StatusId = Status.Approved,
                            ApprovedDate = DateTime.UtcNow,
                            ApprovalNotes = "Auto-approved leave for absent days",
                            CreatedOn = DateTime.UtcNow,
                            IsActive = true,
                            IsDeleted = false
                        });
            }
        }
    }
}