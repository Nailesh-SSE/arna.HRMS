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
    private readonly IMapper _mapper;
    private readonly IAttendanceService _attendanceService;
    private readonly AttendanceRequestValidator _validator;

    public AttendanceRequestService(
        AttendanceRequestRepository repository,
        IMapper mapper,
        IAttendanceService attendanceService,
        AttendanceRequestValidator validator)
    {
        _repository = repository;
        _mapper = mapper;
        _attendanceService = attendanceService;
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
}