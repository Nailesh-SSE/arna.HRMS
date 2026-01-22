using arna.HRMS.Core.DTOs.Responses;
using arna.HRMS.Core.Entities;
using arna.HRMS.Core.Enums;
using arna.HRMS.Infrastructure.Repositories;
using arna.HRMS.Infrastructure.Services.Interfaces;
using arna.HRMS.Models.DTOs;
using AutoMapper;

namespace arna.HRMS.Infrastructure.Services;

public class AttendanceRequestService : IAttendanceRequestService
{
    private readonly AttendanceRequestRepository _attendanceRequestRepository;
    private readonly IMapper _mapper;

    public AttendanceRequestService(
        AttendanceRequestRepository attendanceRequestRepository,
        IMapper mapper)
    {
        _attendanceRequestRepository = attendanceRequestRepository;
        _mapper = mapper;
    }

    public async Task<ServiceResult<List<AttendanceRequestDto>>> GetAttendanceRequestAsync()
    {
        var attendanceRequests = await _attendanceRequestRepository.GetAttendanceRequestAsync();
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

    public async Task<ServiceResult<bool>> UpdateAttendanceRequestStatusAsync(int id, CommonStatusList status, int approvedBy)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid AttendanceRequest ID");

        var updated = await _attendanceRequestRepository.UpdateAttendanceRequestStatusAsync(id, status, approvedBy);

        return ServiceResult<bool>.Success(updated);
    }

    public async Task<ServiceResult<bool>> UpdateAttendanceRequestStatusCancleAsync(int id, int EmployeeId)
    {
        if (id <= 0)
            return ServiceResult<bool>.Fail("Invalid AttendanceRequest ID");
        var updated = await _attendanceRequestRepository.GetAttendanceRequestCancelAsync(id, EmployeeId);
        return ServiceResult<bool>.Success(updated);
    }
}
