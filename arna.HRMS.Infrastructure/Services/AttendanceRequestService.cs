using arna.HRMS.Core.Common.Results;
using arna.HRMS.Core.DTOs;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Core.Interfaces.Service;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Validators;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly AttendanceRequestRepository _repository;
    private readonly IMapper _mapper;
    private readonly IAttendanceService _attendanceService;
    private readonly ILeaveService _leaveService;
    private readonly AttendanceRequestValidator _validator;

    public AttendanceRequestService(
        AttendanceRequestRepository repository,
        IMapper mapper,
        IAttendanceService attendanceService,
        ILeaveService leaveService,
        AttendanceRequestValidator validator)
    {
        _repository = repository;
        _mapper = mapper;
        _attendanceService = attendanceService;
        _leaveService = leaveService;
        _validator = validator;
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequestsAsync(int? employeeId, Status? status)
    {
        var data = await _repository.GetAttendanceRequests(employeeId, status);
        var mapped = _mapper.Map<List<AttendanceRequestDto>>(data);
        var response = ServiceResult<List<AttendanceRequestDto>>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetPendingAttendanceRequestsAsync()
    {
        var data = await _repository.GetPendingAttendanceRequests();
        var mapped = _mapper.Map<List<AttendanceRequestDto>>(data);
        var response = ServiceResult<List<AttendanceRequestDto>>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<AttendanceRequestDto?>> GetAttendanceRequestByIdAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<AttendanceRequestDto?>.Fail("Invalid AttendanceRequest ID");
            return failId;
        }

        var entity = await _repository.GetAttendanceRequestByIdAsync(id);
        if (entity == null)
        {
            var failNotFound = ServiceResult<AttendanceRequestDto?>.Fail("Attendance request not found");
            return failNotFound;
        }

        var mapped = _mapper.Map<AttendanceRequestDto>(entity);
        var response = ServiceResult<AttendanceRequestDto?>.Success(mapped);
        return response;
    }

    public async Task<ServiceResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto dto)
    {
        var validation = await _validator.ValidateCreateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<AttendanceRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var entity = _mapper.Map<AttendanceRequest>(dto);
        var created = await _repository.CreateAttendanceRequestAsync(entity);
        var mapped = _mapper.Map<AttendanceRequestDto>(created);
        var response = ServiceResult<AttendanceRequestDto>.Success(mapped, "Attendance request created successfully");
        return response;
    }

    public async Task<ServiceResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(AttendanceRequestDto dto)
    {
        var validation = await _validator.ValidateUpdateAsync(dto);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<AttendanceRequestDto>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var entity = _mapper.Map<AttendanceRequest>(dto);
        var updated = await _repository.UpdateAttendanceRequestAsync(entity);
        var mapped = _mapper.Map<AttendanceRequestDto>(updated);
        var response = ServiceResult<AttendanceRequestDto>.Success(mapped, "Attendance request updated successfully");
        return response;
    }

    public async Task<ServiceResult<bool>> DeleteAttendanceRequestAsync(int id)
    {
        if (id <= 0)
        {
            var failId = ServiceResult<bool>.Fail("Attendance request not found");
            return failId;
        }

        var deleted = await _repository.DeleteAttendanceRequestAsync(id);
        if (deleted)
        {
            var success = ServiceResult<bool>.Success(true, "Attendance request deleted successfully");
            return success;
        }

        var failNotFound = ServiceResult<bool>.Fail("Attendance request not found");
        return failNotFound;
    }

    public async Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id, Status status, int approvedBy)
    {
        var validation = await _validator.ValidateStatusUpdateAsync(id, status, approvedBy);
        if (!validation.IsValid)
        {
            var failVal = ServiceResult<bool>.Fail(string.Join(Environment.NewLine, validation.Errors));
            return failVal;
        }

        var updated = await _repository.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);
        if (!updated)
        {
            var failUpdate = ServiceResult<bool>.Fail("Failed to update attendance request status.");
            return failUpdate;
        }

        if (status == Status.Approved)
        {
            var request = await _repository.GetAttendanceRequestByIdAsync(id);
            if (request != null)
            {
                await CreateAttendanceFromRequestAsync(_mapper.Map<AttendanceRequestDto>(request));
            }
        }

        var response = ServiceResult<bool>.Success(true, "Attendance request status updated successfully");
        return response;
    }

    public async Task<ServiceResult<int>> BulkApproveAttendanceRequestsAsync(List<int> requestIds, int approvedBy)
    {
        if (requestIds == null || !requestIds.Any() || approvedBy <= 0)
        {
            var failData = ServiceResult<int>.Fail("Invalid request data.");
            return failData;
        }

        var approvedEntities = await _repository.BulkApproveAttendanceRequestsAsync(requestIds, approvedBy);
        if (!approvedEntities.Any())
        {
            var failNone = ServiceResult<int>.Fail("No pending attendance requests were found to approve");
            return failNone;
        }

        var dtos = approvedEntities.Select(ar => _mapper.Map<AttendanceRequestDto>(ar)).ToList();
        await Task.WhenAll(dtos.Select(dto => CreateAttendanceFromRequestAsync(dto)));

        var response = ServiceResult<int>.Success(approvedEntities.Count, $"Successfully approved {approvedEntities.Count} attendance request(s).");
        return response;
    }

    public async Task<ServiceResult<bool>> CancelAttendanceRequestAsync(int id, int employeeId)
    {
        if (id <= 0 || employeeId <= 0)
        {
            var failReq = ServiceResult<bool>.Fail("Invalid request");
            return failReq;
        }

        var cancelled = await _repository.CancelAttendanceRequestAsync(id, employeeId);
        if (cancelled)
        {
            var success = ServiceResult<bool>.Success(true, "Attendance request cancelled successfully");
            return success;
        }

        var failCancel = ServiceResult<bool>.Fail("Failed to cancel attendance request");
        return failCancel;
    }

    private async Task CreateAttendanceFromRequestAsync(AttendanceRequestDto req)
    {
        var fromDate = req.FromDate!.Value.Date;
        var toDate = req.ToDate!.Value.Date;

        var clockIn = req.ClockIn!.Value.TimeOfDay;
        var clockOut = req.ClockOut!.Value.TimeOfDay;

        await _leaveService.RevokeLeaveRequestAsync(req.EmployeeId, fromDate, toDate);

        var requestDates = Enumerable.Range(0, (toDate - fromDate).Days + 1)
            .Select(offset => fromDate.AddDays(offset))
            .Where(d => fromDate == toDate || d.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday));

        var workHours = req.TotalHours > TimeSpan.Zero
            ? req.TotalHours
            : (clockOut > clockIn ? clockOut - clockIn : TimeSpan.Zero);

        await requestDates.Aggregate(Task.CompletedTask, async (prev, d) =>
        {
            await prev;
            await InsertAttendanceAsync(req, d, clockIn, clockOut, workHours);
        });
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
            Notes = string.IsNullOrWhiteSpace(req.Description) ? "Attendance Request Approved" : req.Description,
            DeviceId = DeviceType.LaptopDesktop
        });
    }
}
