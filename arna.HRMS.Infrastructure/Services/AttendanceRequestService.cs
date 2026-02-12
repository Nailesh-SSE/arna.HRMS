using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly AttendanceRequestRepository _attendanceRequestRepository;
    private readonly IMapper _mapper;
    private readonly IAttendanceService _attendanceService;
    private readonly AttendanceRequestValidator _validator;

    public AttendanceRequestService(
        AttendanceRequestRepository attendanceRequestRepository,
        IMapper mapper,
        IAttendanceService attendanceService,
        AttendanceRequestValidator validator)
    {
        _attendanceRequestRepository = attendanceRequestRepository;
        _mapper = mapper;
        _attendanceService = attendanceService;
        _validator = validator;
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequests(int? employeeId, Status? status)
    {
        var list = _mapper.Map<List<AttendanceRequestDto>>(
            await _attendanceRequestRepository.GetAttendanceRequests(employeeId, status));

        return ServiceResult<List<AttendanceRequestDto>>.Success(list);
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetPendingAttendanceRequestesAsync()
    {
        var list = _mapper.Map<List<AttendanceRequestDto>>(
            await _attendanceRequestRepository.GetPandingAttendanceRequestes());

        return ServiceResult<List<AttendanceRequestDto>>.Success(list);
    }

    public async Task<ServiceResult<AttendanceRequestDto?>> GetAttendenceRequestByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<AttendanceRequestDto?>.Fail("Invalid AttendanceRequest ID");

        var attendance = await _attendanceRequestRepository.GetAttendanceRequestByIdAsync(id);

        return attendance == null
            ? ServiceResult<AttendanceRequestDto?>.Fail("Attendance request not found")
            : ServiceResult<AttendanceRequestDto?>.Success(_mapper.Map<AttendanceRequestDto>(attendance));
    }

    public async Task<ServiceResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<AttendanceRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var created = await _attendanceRequestRepository.CreateAttendanceRequestAsync(_mapper.Map<AttendanceRequest>(dto));

        return ServiceResult<AttendanceRequestDto>.Success(_mapper.Map<AttendanceRequestDto>(created), "Attendance request created successfully");
    }

    public async Task<ServiceResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(AttendanceRequestDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
            return ServiceResult<AttendanceRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var updated = await _attendanceRequestRepository.UpdateAttendanceRequestAsync(_mapper.Map<AttendanceRequest>(dto));

        return ServiceResult<AttendanceRequestDto>.Success(_mapper.Map<AttendanceRequestDto>(updated), "Attendance request updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteAttendanceRequestAsync(int id)
    {
        var attendance = await GetAttendenceRequestByIdAsync(id);
        if (!attendance.IsSuccess || attendance.Data == null)
            return ServiceResult<bool>.Fail("Attendance request not found");

        var deleted = await _attendanceRequestRepository.DeleteAttendanceRequestAsync(id);

        return deleted
            ? ServiceResult<bool>.Success(true, "Attendance request deleted successfully")
            : ServiceResult<bool>.Fail("Attendance request not found");
    }

    public async Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id, Status status, int approvedBy)
    {
        var validation = await _validator.ValidateStatusUpdateAsync(id, status, approvedBy);
        if (!validation.IsValid)
            return ServiceResult<bool>.Fail(string.Join(Environment.NewLine, validation.Errors));

        var updated = await _attendanceRequestRepository.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);

        if (!updated)
            return ServiceResult<bool>.Fail("Failed to update attendance request status");

        if (status == Status.Approved)
        {
            var request = await _attendanceRequestRepository.GetAttendanceRequestByIdAsync(id);
            if (request != null)
                await CreateAttendanceFromRequestAsync(_mapper.Map<AttendanceRequestDto>(request));
        }

        return ServiceResult<bool>.Success(true, "Attendance request status updated successfully");
    }

    public async Task<ServiceResult<bool>> UpdateAttendanceRequestStatusCancleAsync(int id, int employeeId)
    {
        if (id <= 0 || employeeId <= 0)
            return ServiceResult<bool>.Fail("Invalid request");

        var updated = await _attendanceRequestRepository.GetAttendanceRequestCancelAsync(id, employeeId);

        return updated
            ? ServiceResult<bool>.Success(true, "Attendance request cancelled successfully")
            : ServiceResult<bool>.Fail("Failed to cancel attendance request");
    }

    // ==============================
    // BUSINESS LOGIC (UNCHANGED)
    // ==============================

    private async Task CreateAttendanceFromRequestAsync(AttendanceRequestDto req)
    {
        var fromDate = req.FromDate!.Value.Date;
        var toDate = req.ToDate!.Value.Date;

        var clockIn = req.ClockIn!.Value.TimeOfDay;
        var clockOut = req.ClockOut!.Value.TimeOfDay;

        if (fromDate == toDate)
        {
            await InsertAttendance(req, fromDate, clockIn, null, TimeSpan.Zero, null, null);
            await InsertAttendance(req, fromDate, null, clockOut, req.TotalHours, null, null);
            return;
        }

        await InsertAttendance(req, fromDate, clockIn, null, TimeSpan.Zero, null, null);
        await InsertAttendance(req, toDate, null, clockOut, clockOut, null, null);
    }

    private async Task InsertAttendance(
        AttendanceRequestDto req,
        DateTime date,
        TimeSpan? clockIn,
        TimeSpan? clockOut,
        TimeSpan totalHours,
        double? latitude,
        double? longitude)
    {
        await _attendanceService.CreateAttendanceAsync(new AttendanceDto
        {
            EmployeeId = req.EmployeeId,
            Date = date,
            ClockInTime = clockIn,
            ClockOutTime = clockOut,
            WorkingHours = totalHours,
            StatusId = AttendanceStatus.Present,
            Notes = req.Description ?? string.Empty,
            Latitude = latitude,
            Longitude = longitude
        });
    }
}
