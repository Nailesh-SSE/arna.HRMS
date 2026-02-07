using arna.HRMS.Core.Common.ServiceResult;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly AttendanceRequestRepository _attendanceRequestRepository;
    private readonly IMapper _mapper;
    private readonly IAttendanceService _attendanceService;

    public AttendanceRequestService(
        AttendanceRequestRepository attendanceRequestRepository,
        IMapper mapper,
        IAttendanceService attendanceService)
    {
        _attendanceRequestRepository = attendanceRequestRepository;
        _mapper = mapper;
        _attendanceService = attendanceService;
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequests(int? employeeId, Status? status)
    {
        var attendanceRequests = await _attendanceRequestRepository.GetAttendanceRequests(employeeId, status);
        var list = _mapper.Map<List<AttendanceRequestDto>>(attendanceRequests);

        return ServiceResult<List<AttendanceRequestDto>>.Success(list); 
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetPendingAttendanceRequestesAsync()
    {
        var attendanceRequests = await _attendanceRequestRepository.GetPandingAttendanceRequestes();
        var list = _mapper.Map<List<AttendanceRequestDto>>(attendanceRequests);
        return ServiceResult<List<AttendanceRequestDto>>.Success(list);
    }

    public async Task<ServiceResult<AttendanceRequestDto?>> GetAttendenceRequestByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<AttendanceRequestDto?>.Fail("Invalid AttendanceRequest ID");

        var attendance = await _attendanceRequestRepository.GetAttendanceRequestByIdAsync(id);

        if (attendance == null)
            return ServiceResult<AttendanceRequestDto?>.Fail("Attendance request not found");

        var dto = _mapper.Map<AttendanceRequestDto>(attendance);
        return ServiceResult<AttendanceRequestDto?>.Success(dto);
    }

    public async Task<ServiceResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto attendanceRequestDto)
    {
        if (attendanceRequestDto == null)
            return ServiceResult<AttendanceRequestDto>.Fail("Invalid request");

        if (attendanceRequestDto.EmployeeId <= 0)
            return ServiceResult<AttendanceRequestDto>.Fail("EmployeeId is required");

        var entity = _mapper.Map<AttendanceRequest>(attendanceRequestDto);

        var createdAttendanceRequest =
            await _attendanceRequestRepository.CreateAttendanceRequestAsync(entity);

        var resultDto = _mapper.Map<AttendanceRequestDto>(createdAttendanceRequest);

        return ServiceResult<AttendanceRequestDto>.Success(resultDto, "Attendance request created successfully");
    }

    public async Task<ServiceResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(AttendanceRequestDto dto)
    {
        if (dto == null)
            return ServiceResult<AttendanceRequestDto>.Fail("Invalid request");

        if (dto.Id <= 0)
            return ServiceResult<AttendanceRequestDto>.Fail("Invalid Attendance Request ID");

        var Attendance = _mapper.Map<AttendanceRequest>(dto);
        var updated = await _attendanceRequestRepository.UpdateAttendanceRequestAsync(Attendance);
        var resultDto = _mapper.Map<AttendanceRequestDto>(updated);

        return ServiceResult<AttendanceRequestDto>.Success(resultDto, "Request updated successfully");
    }

    public async Task<ServiceResult<bool>> DeleteAttendanceRequestAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid AttendanceRequest ID");

        var deleted = await _attendanceRequestRepository.DeleteAttendanceRequestAsync(id);
        return ServiceResult<bool>.Success(deleted, "Attendance request deleted successfully");
    }

    public async Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id, Status status, int approvedBy)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid AttendanceRequest ID");

        var updated = await _attendanceRequestRepository.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);

        var attendanceRequest = await _attendanceRequestRepository.GetAttendanceRequestByIdAsync(id);
        var dto = _mapper.Map<AttendanceRequestDto>(attendanceRequest);
        if (status == Status.Approved && updated)
        {
            await CreateAttendanceFromRequestAsync(dto);
        }

        return ServiceResult<bool>.Success(updated);
    }

    private async Task CreateAttendanceFromRequestAsync(AttendanceRequestDto req)
    {
        var fromDate = req.FromDate!.Value.Date;
        var toDate = req.ToDate!.Value.Date;

        var clockIn = req.ClockIn!.Value.TimeOfDay;
        var clockOut = req.ClockOut!.Value.TimeOfDay;

        if (fromDate == toDate)
        {
            var existingClockInOnly =
                (await _attendanceService.GetAttendanceAsync())
                    .Data?
                    .FirstOrDefault(a =>
                        a.EmployeeId == req.EmployeeId &&
                        a.Date.Date == fromDate &&
                        a.ClockInTime.HasValue &&
                        !a.ClockOutTime.HasValue &&
                        a.ClockInTime.Value.Hours == clockIn.Hours &&
                        a.ClockInTime.Value.Minutes == clockIn.Minutes
                    );

            if (existingClockInOnly != null)
            {
                await InsertAttendance(
                    req,
                    fromDate,
                    null,
                    clockOut,
                    (clockOut - (existingClockInOnly.ClockInTime ?? TimeSpan.Zero)),
                    null,
                    null
                );
                return;
            }

            await InsertAttendance(req, fromDate, clockIn, null, TimeSpan.Zero, null, null);
            await InsertAttendance(req, fromDate, null, clockOut, req.TotalHours, null, null);
            return;
        }

        await InsertAttendance(req, fromDate, clockIn, null, TimeSpan.Zero, null, null);

        await InsertAttendance(
            req,
            fromDate,
            null,
            new TimeSpan(23, 59, 59),
            new TimeSpan(23, 59, 59) - clockIn,
            null,
            null
        );

        await InsertAttendance(
            req,
            toDate,
            TimeSpan.Zero,
            null,
            TimeSpan.Zero,
            null,
            null
        );

        await InsertAttendance(
            req,
            toDate,
            null,
            clockOut,
            clockOut,
            null,
            null
        );
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
        var attendance = new AttendanceDto
        {
            EmployeeId = req.EmployeeId,
            Date = date,
            ClockInTime = clockIn,
            ClockOutTime = clockOut,
            WorkingHours = totalHours,
            StatusId = AttendanceStatus.Present,
            Notes = req.Description,
            Latitude = latitude,
            Longitude = longitude
        };

        await _attendanceService.CreateAttendanceAsync(attendance);
    }


    public async Task<ServiceResult<bool>> UpdateAttendanceRequestStatusCancleAsync(int id, int EmployeeId)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid AttendanceRequest ID");
        var updated = await _attendanceRequestRepository.GetAttendanceRequestCancelAsync(id, EmployeeId);
        return ServiceResult<bool>.Success(updated);
    }
}
