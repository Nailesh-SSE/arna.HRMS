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

        return list.Any()
            ? ServiceResult<List<AttendanceRequestDto>>.Success(list)
            : ServiceResult<List<AttendanceRequestDto>>.Fail("No Data Found"); 
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetPendingAttendanceRequestesAsync()
    {
        var attendanceRequests = await _attendanceRequestRepository.GetPandingAttendanceRequestes();
        var list = _mapper.Map<List<AttendanceRequestDto>>(attendanceRequests);
        return list.Any()
            ? ServiceResult<List<AttendanceRequestDto>>.Success(list)
            : ServiceResult<List<AttendanceRequestDto>>.Fail("No Data Found");
    }

    public async Task<ServiceResult<AttendanceRequestDto?>> GetAttendenceRequestByIdAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<AttendanceRequestDto?>.Fail("Invalid AttendanceRequest ID");

        var attendance = await _attendanceRequestRepository.GetAttendanceRequestByIdAsync(id);

        if (attendance == null)
            return ServiceResult<AttendanceRequestDto?>.Fail("Attendance request not found");

        var dto = _mapper.Map<AttendanceRequestDto>(attendance);
        return dto != null
            ? ServiceResult<AttendanceRequestDto?>.Success(dto)
            : ServiceResult<AttendanceRequestDto?>.Fail("Fail to Find Attendance Request") ;
    }

    public async Task<ServiceResult<AttendanceRequestDto>> CreateAttendanceRequestAsync(AttendanceRequestDto attendanceRequestDto)
    {
        if (attendanceRequestDto == null)
            return ServiceResult<AttendanceRequestDto>.Fail("Invalid request");

        if (attendanceRequestDto.EmployeeId <= 0)
            return ServiceResult<AttendanceRequestDto>.Fail("EmployeeId is required");

        if (attendanceRequestDto.ClockIn == default)
            return ServiceResult<AttendanceRequestDto>.Fail("ClockIn is required");
        if (attendanceRequestDto.ClockOut == default)
            return ServiceResult<AttendanceRequestDto>.Fail("ClockOut is required");

        if (attendanceRequestDto.LocationId == default)
            return ServiceResult<AttendanceRequestDto>.Fail("Location is required");
        if (attendanceRequestDto.ReasonTypeId == default)
            return ServiceResult<AttendanceRequestDto>.Fail("Reson is required");

        if (attendanceRequestDto.FromDate == default)
            return ServiceResult<AttendanceRequestDto>.Fail("From Date is required");
        if (attendanceRequestDto.ToDate == default)
            return ServiceResult<AttendanceRequestDto>.Fail("To Date is required");
        if (attendanceRequestDto.FromDate.Value.Date >= DateTime.Now.Date)
            return ServiceResult<AttendanceRequestDto>.Fail("Future or current date is not allowed.");
        if (attendanceRequestDto.ToDate.Value.Date >= DateTime.Now.Date)
            return ServiceResult<AttendanceRequestDto>.Fail("Future or current date is not allowed.");
        var entity = _mapper.Map<AttendanceRequest>(attendanceRequestDto);

        var createdAttendanceRequest =
            await _attendanceRequestRepository.CreateAttendanceRequestAsync(entity);

        var resultDto = _mapper.Map<AttendanceRequestDto>(createdAttendanceRequest);

        return resultDto!=null
            ? ServiceResult<AttendanceRequestDto>.Success(resultDto, "Attendance request created successfully")
            : ServiceResult<AttendanceRequestDto>.Fail("Fail to create");
    }

    public async Task<ServiceResult<AttendanceRequestDto>> UpdateAttendanceRequestAsync(AttendanceRequestDto dto)
    {
        if (dto == null)
            return ServiceResult<AttendanceRequestDto>.Fail("Invalid request");

        if (dto.Id <= 0)
            return ServiceResult<AttendanceRequestDto>.Fail("Invalid Attendance Request ID");
        var exist = GetAttendenceRequestByIdAsync(dto.Id);
        if(exist==null)
            return ServiceResult<AttendanceRequestDto>.Fail("Invalid Attendance Request ID");

        if (dto.EmployeeId <= 0)
            return ServiceResult<AttendanceRequestDto>.Fail("EmployeeId is required");

        if (dto.ClockIn == default)
            return ServiceResult<AttendanceRequestDto>.Fail("ClockIn is required");
        if (dto.ClockOut == default)
            return ServiceResult<AttendanceRequestDto>.Fail("ClockOut is required");

        if (dto.LocationId == default)
            return ServiceResult<AttendanceRequestDto>.Fail("Location is required");
        if (dto.ReasonTypeId == default)
            return ServiceResult<AttendanceRequestDto>.Fail("Reson is required");

        if (dto.FromDate == default)
            return ServiceResult<AttendanceRequestDto>.Fail("From Date is required");
        if (dto.ToDate == default)
            return ServiceResult<AttendanceRequestDto>.Fail("To Date is required");
        if (dto.FromDate.Value.Date >= DateTime.Now.Date)
            return ServiceResult<AttendanceRequestDto>.Fail("Future or current date is not allowed.");
        if (dto.ToDate.Value.Date >= DateTime.Now.Date)
            return ServiceResult<AttendanceRequestDto>.Fail("Future or current date is not allowed.");


        var Attendance = _mapper.Map<AttendanceRequest>(dto);
        var updated = await _attendanceRequestRepository.UpdateAttendanceRequestAsync(Attendance);
        var resultDto = _mapper.Map<AttendanceRequestDto>(updated);

        return resultDto != null
            ? ServiceResult<AttendanceRequestDto>.Success(resultDto, "Request updated successfully")
            : ServiceResult<AttendanceRequestDto>.Fail("Fail to Update Attendance Form");
    }

    public async Task<ServiceResult<bool>> DeleteAttendanceRequestAsync(int id)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid AttendanceRequest ID");

        var deleted = await _attendanceRequestRepository.DeleteAttendanceRequestAsync(id);
        return deleted
            ? ServiceResult<bool>.Success(deleted, "Attendance request deleted successfully")
            : ServiceResult<bool>.Fail("Attendance request Fail to deleted");
    }

    public async Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id, Status status, int approvedBy)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid AttendanceRequest ID");
        var exist = GetAttendenceRequestByIdAsync(id);
        if (exist == null)
            return ServiceResult<bool>.Fail("Invalid Attendance Request ID");

        if (status == Status.Cancelled)
            return ServiceResult<bool>.Fail("Not Allow to Cancle");
        if (status == Status.Pending)
            return ServiceResult<bool>.Fail("Not Allow to Pending");

        if (approvedBy <= 0)
            return ServiceResult<bool>.Fail("Invalid Approved ID");
        var updated = await _attendanceRequestRepository.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);

        var attendanceRequest = await _attendanceRequestRepository.GetAttendanceRequestByIdAsync(id);
        var dto = _mapper.Map<AttendanceRequestDto>(attendanceRequest);
        if (status == Status.Approved && updated)
        {
            await CreateAttendanceFromRequestAsync(dto);
        }

        return updated
            ? ServiceResult<bool>.Success(updated)
            : ServiceResult<bool>.Fail("Fail to Update Status");
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

        var exist = GetAttendenceRequestByIdAsync(id);
        if (exist == null)
            return ServiceResult<bool>.Fail("Invalid Attendance Request ID");
        if (EmployeeId<=0)
            return ServiceResult<bool>.Fail("Invalid Employee");
        var updated = await _attendanceRequestRepository.GetAttendanceRequestCancelAsync(id, EmployeeId);
        return updated
            ? ServiceResult<bool>.Success(updated)
            : ServiceResult<bool>.Fail("Fail to Update Status");
    }
}
